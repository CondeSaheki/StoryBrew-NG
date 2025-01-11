using Newtonsoft.Json;

namespace StoryBrew.Files;

internal class ProjectConfiguration
{
    private static readonly Version lastest_version = new(1, 0, 0);
    public Version Version { get; set; }

    public string MapsetDirectoryPath { get; set; } = string.Empty;

    // layers
    public List<ScriptConfiguration> Background { get; set; } = [];
    public List<ScriptConfiguration> Fail { get; set; } = [];
    public List<ScriptConfiguration> Pass { get; set; } = [];
    public List<ScriptConfiguration> Foreground { get; set; } = [];
    public List<ScriptConfiguration> Overlay { get; set; } = [];
    public List<ScriptConfiguration> Video { get; set; } = [];

    public ProjectConfiguration()
    {
        Version = lastest_version;
    }

    internal static ProjectConfiguration FromFile(string path)
    {
        var version = Version.FromJsonFile(path);

        Func<string, ProjectConfiguration> handler = version.ToTuple() switch
        {
            (1, 0, 0) => version_handler_1_0_0,
            _ => throw new Exception($"Configuration version {version} is not compatible.")
        };
        return handler.Invoke(path);
    }

    private static ProjectConfiguration version_handler_1_0_0(string path)
    {
        var configRaw = File.ReadAllText(path);
        var config = JsonConvert.DeserializeObject<ProjectConfiguration>(configRaw) ?? throw new Exception("Failed to deserialize user file.");
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

internal class ScriptConfiguration
{
    public string Nickname { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<Configurable> Configurables { get; set; } = [];
}
