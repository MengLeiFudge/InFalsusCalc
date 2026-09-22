using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace InFalsusCalc;

/// <summary>项目自有路径、JSON格式与原子落盘，不接触游戏存档。</summary>
internal static class Storage
{
    /// <summary>布局格式保留蛇形字段，与旧数据和网页一致。</summary>
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
    /// <summary>串行化检查点读写，避免状态读取与Windows文件替换争用句柄。</summary>
    private static readonly object IoGate = new();
    /// <summary>仓库工作目录；独立构建输出则使用可执行文件目录。</summary>
    public static readonly string Root = FindRoot();
    /// <summary>计算器输入资源目录，不包含网页和图片。</summary>
    public static string Input => Directory.Exists(Path.Combine(Root, "Calculator", "Data"))
        ? Path.Combine(Root, "Calculator", "Data") : Path.Combine(Root, "Data");
    /// <summary>本程序的检查点和运行状态目录。</summary>
    public static string State => Path.Combine(Root, "state");

    /// <summary>确定可执行文件所属项目根，不依赖启动终端的当前目录。</summary>
    /// <returns>项目根或便携分发目录。</returns>
    private static string FindRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Calculator", "InFalsusCalc.csproj")))
                return current.FullName;
            current = current.Parent;
        }
        return AppContext.BaseDirectory;
    }

    /// <summary>读取已存在的项目JSON，缺失文件返回空值，格式损坏直接报告。</summary>
    /// <typeparam name="T">目标数据类型。</typeparam>
    /// <param name="path">JSON绝对路径。</param>
    /// <returns>数据对象或文件不存在时的空值。</returns>
    public static T? Read<T>(string path) where T : class
    {
        lock (IoGate)
        {
            if (!File.Exists(path))
                return null;
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            return JsonSerializer.Deserialize<T>(stream, Json) ?? throw new InvalidDataException($"JSON为空：{path}");
        }
    }

    /// <summary>原子替换单个JSON文件，中断不会留下半个检查点。</summary>
    /// <typeparam name="T">可序列化的数据类型。</typeparam>
    /// <param name="path">项目自有输出路径。</param>
    /// <param name="value">需要保存的数据。</param>
    public static void Write<T>(string path, T value)
    {
        lock (IoGate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temporary = path + $".{Environment.ProcessId}.{Environment.CurrentManagedThreadId}.tmp";
            using (FileStream stream = new(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, value, Json);
                stream.Flush(true);
            }
            Exception? last = null;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                try
                {
                    if (File.Exists(path))
                        File.Replace(temporary, path, null, true);
                    else
                        File.Move(temporary, path);
                    return;
                }
                catch (IOException error) when (attempt < 7) { last = error; Thread.Sleep(100); }
                catch (UnauthorizedAccessException error) when (attempt < 7) { last = error; Thread.Sleep(100); }
            }
            throw new IOException($"无法替换成果文件：{path}。", last);
        }
    }

    /// <summary>按紧凑JSON的SHA-256生成稳定标识，不使用进程随机哈希。</summary>
    /// <param name="value">数组等稳定顺序的数据。</param>
    /// <returns>24个十六进制字符。</returns>
    public static string Digest(object value)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, Json);
        return Convert.ToHexStringLower(SHA256.HashData(bytes))[..24];
    }

    /// <summary>取得计算锁后，从仓库快照补齐本地缺失的检查点，不覆盖续算进度。</summary>
    /// <returns>调用方持有至计算结束的文件句柄。</returns>
    public static FileStream AcquireLock()
    {
        Directory.CreateDirectory(State);
        FileStream lease;
        try
        {
            lease = new FileStream(Path.Combine(State, "calculation.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException error) { throw new InvalidOperationException("本项目已有计算进程在运行。", error); }
        try
        {
            string snapshot = Path.Combine(Root, "checkpoints");
            if (Directory.Exists(snapshot))
                foreach (string file in Directory.EnumerateFiles(snapshot, "*.json", SearchOption.AllDirectories))
                {
                    string target = Path.Combine(State, Path.GetRelativePath(snapshot, file));
                    if (File.Exists(target))
                        continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    string temporary = target + $".{Environment.ProcessId}.tmp";
                    File.Copy(file, temporary, true);
                    File.Move(temporary, target);
                }
            return lease;
        }
        catch { lease.Dispose(); throw; }
    }
}

/// <summary>读取已提取的真实资源快照。</summary>
internal sealed class Catalog
{
    /// <summary>带类型的游戏数据。</summary>
    public GameSnapshot Data
    {
        get;
    }
    /// <summary>保留原始结构供网页绘制，不改变资源字段名。</summary>
    public JsonElement Raw
    {
        get;
    }
    /// <summary>粒子快速查找表。</summary>
    public IReadOnlyDictionary<int, Shape> Shapes
    {
        get;
    }
    /// <summary>特性快速查找表。</summary>
    public IReadOnlyDictionary<int, SkillTrait> Traits
    {
        get;
    }
    /// <summary>加载当前资源快照。</summary>
    public Catalog()
    {
        string path = Path.Combine(Storage.Input, "catalog.json");
        Data = Storage.Read<GameSnapshot>(path) ?? throw new FileNotFoundException("缺少游戏资源快照。", path);
        using (JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(path)))
            Raw = document.RootElement.Clone();
        if (!Data.Compatible || Data.Rules != "007d2f8-v1")
            throw new InvalidDataException("资源快照规则版本不可用。");
        Shapes = Data.Shapes.ToDictionary(shape => shape.Id);
        Traits = Data.Traits.ToDictionary(trait => trait.Id);
    }

}
