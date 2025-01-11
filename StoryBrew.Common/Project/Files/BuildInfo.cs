using Newtonsoft.Json;

namespace StoryBrew.Files;

internal class BuildInfo
{
    private static readonly Version lastest_version = new(1, 0, 0);
    public Version Version { get; set; }

    public HashSet<string> Hashes { get; set; } = [];

    public Dictionary<string, List<ConfigurableInfo>> ScriptsInfo { get; set; } = [];

    public BuildInfo()
    {
        Version = lastest_version;
    }

    public static BuildInfo FromFile(string filePath)
    {
        if (Version.FromJsonFile(filePath) != lastest_version)
        {
            File.Delete(filePath);
            return new BuildInfo();
        }

        var hashesRaw = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<BuildInfo>(hashesRaw) ?? new BuildInfo();
    }

    public void Save(string path, bool overwrite = false)
    {
        if (!overwrite && File.Exists(path)) throw new ArgumentException($"The config file already exists at {path}.");

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}
