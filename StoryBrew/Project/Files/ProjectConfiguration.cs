using Newtonsoft.Json;
using StoryBrew.Scripting;

namespace StoryBrew.Project.Files;

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

    internal static ProjectConfiguration FromFile(string filePath)
    {
        var version = Version.FromJsonFile(filePath);

        Func<string, ProjectConfiguration> handler = version.ToTuple() switch
        {
            (1, 0, 0) => version_handler_1_0_0,
            _ => throw new Exception($"Configuration version {version} is not compatible.")
        };
        return handler.Invoke(filePath);
    }

    private static ProjectConfiguration version_handler_1_0_0(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        using var streamReader = new StreamReader(fileStream);
        using var jsonReader = new JsonTextReader(streamReader);

        var config = JsonSerializer.CreateDefault().Deserialize<ProjectConfiguration>(jsonReader)
            ?? throw new Exception("Failed to deserialize user file.");

        config.Version = lastest_version;
        return config;
    }

    internal void Save(string filePath, bool overwrite = false)
    {
        if (!overwrite && File.Exists(filePath)) throw new ArgumentException($"The config file already exists at {filePath}.");

        using var fileStream = File.Create(filePath);
        using var streamWriter = new StreamWriter(fileStream);
        using var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };

        JsonSerializer.CreateDefault().Serialize(jsonWriter, this);
    }
}

internal class ScriptConfiguration
{
    public string Nickname { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<Configurable> Configurables { get; set; } = [];
}
