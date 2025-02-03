using Newtonsoft.Json;
using StoryBrew.Scripting;

namespace StoryBrew.Project.Files;

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

        using var fileStream = File.OpenRead(filePath);
        using var streamReader = new StreamReader(fileStream);
        using var jsonReader = new JsonTextReader(streamReader);

        return JsonSerializer.CreateDefault().Deserialize<BuildInfo>(jsonReader) ?? new BuildInfo();
    }

    public void Save(string filePath, bool overwrite = false)
    {
        if (!overwrite && File.Exists(filePath)) throw new ArgumentException($"The config file already exists at {filePath}.");

        using var fileStream = File.Create(filePath);
        using var streamWriter = new StreamWriter(fileStream);
        using var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };

        JsonSerializer.CreateDefault().Serialize(jsonWriter, this);
    }
}
