/*
    // Note: Configurations that existed in old StoryBrew that are not implemented or may not be implemented in the future:

    /// <summary>
    /// Not compatible with Fallback!
    /// </summary>
    public bool UseFloatForMove = true;

    /// <summary>
    /// Not compatible with Stable!
    /// </summary>
    public bool UseFloatForTime = false;

    /// <summary>
    /// Enables optimisation for OsbSprites that have a MaxCommandCount > 0
    /// </summary>
    public bool OptimiseSprites = true;

    public readonly NumberFormatInfo NumberFormat = new CultureInfo(@"en-US", false).NumberFormat;
    
*/

using Newtonsoft.Json;

namespace StoryBrew;

public class ProjectData
{
    public readonly Version Version = Bootstrap.VERSION;

    public string MapsetDirectoryPath { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;

    public ProjectLayers Layer { get; set; } = new();

    internal ProjectData() { }

    public static ProjectData FromFile(in string filePath)
    {
        var version = Version.FromJsonFile(filePath);

        if (version != Bootstrap.VERSION) throw new Exception($"Configuration version {version} is not compatible.");

        using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
        using StreamReader streamReader = new(fileStream);
        using JsonTextReader jsonReader = new(streamReader);

        var config = JsonSerializer.CreateDefault().Deserialize<ProjectData>(jsonReader)
            ?? throw new Exception("Failed to deserialize project data file.");

        return config;
    }

    public static ProjectData FromString(in string content)
    {
        return JsonConvert.DeserializeObject<ProjectData>(content) ?? throw new Exception("Failed to deserialize project data file.");
    }

    public void Save(string filePath, bool overwrite = false)
    {
        if (!overwrite && File.Exists(filePath)) throw new ArgumentException($"The config file already exists at {filePath}.");

        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write);
        using StreamWriter streamWriter = new(fileStream);
        using JsonTextWriter jsonWriter = new(streamWriter) { Formatting = Formatting.Indented };

        JsonSerializer.CreateDefault().Serialize(jsonWriter, this);
    }
}

public struct ScriptData(string fullName, string json)
{
    public string FullName = fullName;
    public string Json = json;
}

public struct ProjectLayers()
{
    public List<ScriptData> Background = [];
    public List<ScriptData> Fail = [];
    public List<ScriptData> Pass = [];
    public List<ScriptData> Foreground = [];
    public List<ScriptData> Overlay = [];
    public List<ScriptData> Video = [];
}