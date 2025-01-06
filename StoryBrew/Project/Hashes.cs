
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace StoryBrew;

public class Hashes
{
    private static readonly Version lastest_version = new(1, 0, 0);
    public Version Version { get; set; } = new(null);

    public Dictionary<string, string> ScriptsFiles { get; set; } = [];
    public Dictionary<string, string> ScriptsLibrary { get; set; } = [];

    public Hashes()
    {
        Version = lastest_version;
    }

    public static Hashes FromFile(string filePath)
    {
        if (Version.FromJsonFile(filePath) != lastest_version)
        {
            File.Delete(filePath);
            return new Hashes();
        }

        var hashesRaw = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<Hashes>(hashesRaw) ?? new Hashes();
    }

    public void Save(string path, bool overwrite = false)
    {
        if (!overwrite && File.Exists(path)) throw new ArgumentException($"The config file already exists at {path}.");

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public bool Match(string file, string hash)
    {
        if (ScriptsFiles.TryGetValue(file, out var hashValue) && hashValue == hash) return true;
        return false;
    }

    public bool MatchLibrary(string path)
    {
        var directory = new DirectoryInfo(path);
        var files = directory.GetFiles("*.cs*", SearchOption.AllDirectories).Select(file => file.FullName).ToArray();

        if (ScriptsLibrary.Count != files.Length) return false;

        var library = FileHashes(files);
        foreach (var (filePath, hash) in library)
        {
            if (ScriptsLibrary.TryGetValue(filePath, out var hashValue) && hashValue == hash) continue;

            return false;
        }
        return true;
    }

    public static string FileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static Dictionary<string, string> FileHashes(string[] filePaths)
    {
        var fileHashes = new Dictionary<string, string>(filePaths.Length);
        foreach (var file in filePaths) fileHashes[file] = FileHash(file);
        return fileHashes;
    }
}
