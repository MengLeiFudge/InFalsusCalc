using System.Diagnostics;
using Google.OrTools.Sat;
using Mip = Google.OrTools.LinearSolver.Solver;
using MipVariable = Google.OrTools.LinearSolver.Variable;

namespace InFalsusCalc;

/// <summary>把当前几何模型的同一组约束交给线性整数后端，仅放松连接见证流的整数性。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>求解同一完整几何模型；连续流仅证明可达性，不计为粒子。</summary>
    /// <param name="model">包含全部激活、覆盖、预算及连接条件的原模型。</param>
    /// <param name="selected">原模型的粒子选择变量。</param>
    /// <param name="budget">剩余计算秒数，正无穷表示不设截止。</param>
    /// <returns>原粒子数组中的选中索引与明确求解状态。</returns>
    private (int[] Chosen, CpSolverStatus Status) SolveLinear(CpModel model, BoolVar[] selected, double budget)
    {
        using Mip solver = Mip.CreateSolver("SCIP") ?? throw new InvalidOperationException("缺少SCIP后端。");
        solver.SetNumThreads(threads);
        if (!double.IsPositiveInfinity(budget)) solver.SetTimeLimit((long)Math.Max(1, budget * 1000));
        solver.SetSolverSpecificParametersAsString("limits/solutions = 1\ndisplay/verblevel = 0");
        List<MipVariable> vars = []; List<(double Lo, double Hi)> bounds = [];
        foreach (IntegerVariableProto v in model.Model.Variables)
        {
            if (v.Domain.Count != 2) throw new InvalidDataException("线性后端尚不支持带孔变量域。");
            bool continuous = v.Name.StartsWith("flow", StringComparison.Ordinal) || v.Name.StartsWith("unit", StringComparison.Ordinal) || v.Name.StartsWith("supply", StringComparison.Ordinal);
            vars.Add(continuous ? solver.MakeNumVar(v.Domain[0], v.Domain[1], v.Name) : solver.MakeIntVar(v.Domain[0], v.Domain[1], v.Name));
            bounds.Add((v.Domain[0], v.Domain[1]));
        }
        static void Add(Dictionary<int, double> terms, int variable, double value) => terms[variable] = terms.GetValueOrDefault(variable) + value;
        static Dictionary<int, double> Terms(LinearExpressionProto e)
        {
            Dictionary<int, double> terms = [];
            for (int i = 0; i < e.Vars.Count; i++) Add(terms, e.Vars[i], e.Coeffs[i]);
            return terms;
        }
        (double Lo, double Hi) Range(Dictionary<int, double> terms, double offset)
        {
            double lo = offset, hi = offset;
            foreach (var t in terms)
            {
                var b = bounds[t.Key];
                lo += t.Value * (t.Value >= 0 ? b.Lo : b.Hi);
                hi += t.Value * (t.Value >= 0 ? b.Hi : b.Lo);
            }
            return (lo, hi);
        }
        void Row(Dictionary<int, double> terms, double offset, double lower, double upper, IEnumerable<int> guards)
        {
            int[] literals = guards.ToArray();
            var range = Range(terms, offset);
            foreach (bool low in new[] { true, false })
            {
                double value = low ? lower : upper;
                if (double.IsInfinity(value)) continue;
                Dictionary<int, double> row = new(terms); double constant = offset;
                double slack = low ? Math.Max(0, lower - range.Lo) : Math.Max(0, range.Hi - upper);
                double sign = low ? 1 : -1;
                foreach (int literal in literals)
                {
                    if (literal >= 0) { constant += sign * slack; Add(row, literal, -sign * slack); }
                    else Add(row, -literal - 1, sign * slack);
                }
                var constraint = solver.MakeConstraint(low ? value - constant : double.NegativeInfinity, low ? double.PositiveInfinity : value - constant);
                foreach (var term in row) if (term.Value != 0) constraint.SetCoefficient(vars[term.Key], term.Value);
            }
        }
        foreach (ConstraintProto c in model.Model.Constraints)
        {
            cancellation.ThrowIfCancellationRequested();
            switch (c.ConstraintCase)
            {
                case ConstraintProto.ConstraintOneofCase.Linear:
                    if (c.Linear.Domain.Count != 2) throw new InvalidDataException("线性后端尚不支持带孔线性约束。");
                    Dictionary<int, double> linear = [];
                    for (int i = 0; i < c.Linear.Vars.Count; i++) Add(linear, c.Linear.Vars[i], c.Linear.Coeffs[i]);
                    Row(linear, 0, c.Linear.Domain[0] == long.MinValue ? double.NegativeInfinity : c.Linear.Domain[0],
                        c.Linear.Domain[1] == long.MaxValue ? double.PositiveInfinity : c.Linear.Domain[1], c.EnforcementLiteral);
                    break;
                case ConstraintProto.ConstraintOneofCase.BoolOr:
                case ConstraintProto.ConstraintOneofCase.BoolAnd:
                case ConstraintProto.ConstraintOneofCase.AtMostOne:
                case ConstraintProto.ConstraintOneofCase.ExactlyOne:
                    var literals = c.ConstraintCase switch
                    {
                        ConstraintProto.ConstraintOneofCase.BoolOr => c.BoolOr.Literals,
                        ConstraintProto.ConstraintOneofCase.BoolAnd => c.BoolAnd.Literals,
                        ConstraintProto.ConstraintOneofCase.AtMostOne => c.AtMostOne.Literals,
                        _ => c.ExactlyOne.Literals
                    };
                    Dictionary<int, double> clause = []; double offset = 0;
                    foreach (int literal in literals)
                        if (literal >= 0) Add(clause, literal, 1); else { Add(clause, -literal - 1, -1); offset++; }
                    double lower = c.ConstraintCase == ConstraintProto.ConstraintOneofCase.BoolAnd ? literals.Count : c.ConstraintCase == ConstraintProto.ConstraintOneofCase.AtMostOne ? 0 : 1;
                    double upper = c.ConstraintCase is ConstraintProto.ConstraintOneofCase.AtMostOne or ConstraintProto.ConstraintOneofCase.ExactlyOne ? 1 : double.PositiveInfinity;
                    Row(clause, offset, lower, upper, c.EnforcementLiteral);
                    break;
                case ConstraintProto.ConstraintOneofCase.LinMax:
                    var max = c.LinMax;
                    Dictionary<int, double> target = Terms(max.Target), sum = new(target);
                    double sumOffset = max.Target.Offset;
                    var targetRange = Range(target, max.Target.Offset);
                    if (targetRange.Lo < 0 || targetRange.Hi > 1) throw new InvalidDataException("仅支持布尔最大值。");
                    foreach (LinearExpressionProto e in max.Exprs)
                    {
                        var terms = Terms(e); var range = Range(terms, e.Offset);
                        if (range.Lo < 0 || range.Hi > 1) throw new InvalidDataException("仅支持布尔最大值参数。");
                        Dictionary<int, double> comparison = new(target);
                        foreach (var t in terms) { Add(comparison, t.Key, -t.Value); Add(sum, t.Key, -t.Value); }
                        Row(comparison, max.Target.Offset - e.Offset, 0, double.PositiveInfinity, c.EnforcementLiteral);
                        sumOffset -= e.Offset;
                    }
                    Row(sum, sumOffset, double.NegativeInfinity, 0, c.EnforcementLiteral);
                    break;
                case ConstraintProto.ConstraintOneofCase.Table:
                    if (c.Table.Negated || c.Table.Exprs.Count == 0) throw new InvalidDataException("仅支持显式允许表达式表。");
                    int width = c.Table.Exprs.Count, count = c.Table.Values.Count / width;
                    if (c.Table.Values.Count % width != 0) throw new InvalidDataException("区域表维度不一致。");
                    int[] cases = Enumerable.Range(0, count).Select(i =>
                    {
                        int index = vars.Count; vars.Add(solver.MakeBoolVar($"table{index}")); bounds.Add((0, 1)); return index;
                    }).ToArray();
                    Row(cases.ToDictionary(i => i, _ => 1d), 0, 1, 1, c.EnforcementLiteral);
                    for (int column = 0; column < width; column++)
                    {
                        var e = c.Table.Exprs[column]; var terms = Terms(e);
                        for (int i = 0; i < count; i++) Add(terms, cases[i], -c.Table.Values[i * width + column]);
                        Row(terms, e.Offset, 0, 0, c.EnforcementLiteral);
                    }
                    break;
                default:
                    throw new InvalidDataException($"线性后端未实现约束：{c.ConstraintCase}。");
            }
        }
        if (model.Model.Objective is CpObjectiveProto objective)
        {
            for (int i = 0; i < objective.Vars.Count; i++) solver.Objective().SetCoefficient(vars[objective.Vars[i]], objective.Coeffs[i]);
            solver.Objective().SetMinimization();
        }
        using CancellationTokenRegistration registration = cancellation.Register(() => { solver.InterruptSolve(); });
        using Timer guard = new(_ => { if (cancellation.IsCancellationRequested) solver.InterruptSolve(); }, null, 250, 250);
        Stopwatch watch = Stopwatch.StartNew();
        Mip.ResultStatus status = solver.Solve(); cancellation.ThrowIfCancellationRequested();
        Console.WriteLine($"[{recipe.Id}] 连续流后端：{status}，{watch.Elapsed.TotalSeconds:F3}秒。");
        if (status == Mip.ResultStatus.INFEASIBLE) return ([], CpSolverStatus.Infeasible);
        if (status is not (Mip.ResultStatus.FEASIBLE or Mip.ResultStatus.OPTIMAL)) return ([], CpSolverStatus.Unknown);
        return (Enumerable.Range(0, selected.Length).Where(i => vars[selected[i].Index].SolutionValue() > .5).ToArray(), CpSolverStatus.Feasible);
    }
}
