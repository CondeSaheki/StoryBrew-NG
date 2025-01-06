using Newtonsoft.Json;

namespace StoryBrew;

public class Configuration
{
    private static readonly Version lastest_version = new(1, 0, 0);
    public Version Version { get; set; } = new(null);

    public string Mapset { get; set; } = string.Empty;

    public Dictionary<string, Script> Instances { get; set; } = [];

    // layers
    public List<string> Background { get; set; } = [];
    public List<string> Fail { get; set; } = [];
    public List<string> Pass { get; set; } = [];
    public List<string> Foreground { get; set; } = [];
    public List<string> Overlay { get; set; } = [];
    public List<string> Video { get; set; } = [];

    public Configuration()
    {
        Version = lastest_version;
    }

    internal static Configuration FromFile(string path)
    {
        var version = Version.FromJsonFile(path);

        Func<string, Configuration> handler = version.ToTuple() switch
        {
            (1, 0, 0) => version_handler_1_0_0,
            _ => throw new Exception($"Configuration version {version} is not compatible.")
        };
        return handler.Invoke(path);
    }

    private static Configuration version_handler_1_0_0(string path)
    {
        var configRaw = File.ReadAllText(path);
        var config = JsonConvert.DeserializeObject<Configuration>(configRaw) ?? throw new Exception("Failed to deserialize user file.");
        config.Version = lastest_version;
        return config;
    }

    internal void Save(string path, bool overwrite = false)
    {
        if (!overwrite && File.Exists(path)) throw new ArgumentException($"The config file already exists at {path}.");

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}

public class Script
{
    public string ScriptFile { get; set; } = string.Empty;
    public Dictionary<string, string> Configurations { get; set; } = [];
}
