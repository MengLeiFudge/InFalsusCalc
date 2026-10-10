const PERFORMANCE_DENOMINATOR = 6_000_000;

/** 表现以共同分母的整数表示，让通关奖励和同分比较不受浮点误差影响。 */
export function performanceUnits(score: bigint, clearStatus: "failed" | "cleared"): bigint {
  if (score < 0n) throw new RangeError("Score must be non-negative.");
  if (clearStatus !== "failed" && clearStatus !== "cleared") {
    throw new RangeError("Clear status must be failed or cleared.");
  }
  const bonus = clearStatus === "cleared" ? 1_200_000n : 0n;
  if (score >= 100_000_000n) return 12_000_000n + bonus;
  if (score >= 98_000_000n) return 6_000_000n + (score - 98_000_000n) * 3n + bonus;
  return (score - 95_000_000n) * 2n + bonus;
}

/** 与模组一致的原始表现增量；先比较增量，避免低分截零后失去更好的成绩。 */
export function calculatePerformance(score: bigint, clearStatus: "failed" | "cleared"): number {
  return Number(performanceUnits(score, clearStatus)) / PERFORMANCE_DENOMINATOR;
}

/** 单曲值保持完整精度，通关加 0.2，结果不小于零。 */
export function calculateRating(constant: number, score: bigint, clearStatus: "failed" | "cleared"): number {
  if (!Number.isFinite(constant) || constant < 0) {
    throw new RangeError("Chart constant must be a finite non-negative number.");
  }
  return Math.max(0, constant + calculatePerformance(score, clearStatus));
}

/** 页面与 PNG 均截断到三位；微小补偿仅消除二进制浮点在整数边界的误差。 */
export function formatPotential(value: number): string {
  return (Math.floor(value * 1000 + 1e-9) / 1000).toFixed(3);
}
