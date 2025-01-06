namespace StoryBrew;

public partial class Project
{
    public readonly string ProjectPath;
    public string AssetsLibraryPath => Path.Combine(ProjectPath, "AssetsLibrary");
    public string ScriptsLibraryPath => Path.Combine(ProjectPath, "ScriptsLibrary");

    private Configuration configuration;
    private Hashes hashes;

    private string cacheDirectoryPath => Path.Combine(ProjectPath, ".cache");
    private string configFilePath => Path.Combine(ProjectPath, Path.GetDirectoryName(ProjectPath) ?? string.Empty + ".sbproj");
    private string hashesFilePath => Path.Combine(cacheDirectoryPath, "hashes");

    public Project(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
        {
            throw new ArgumentException("Project path is either null, empty, or does not exist.");
        }
        ProjectPath = projectPath;

        if (!File.Exists(configFilePath)) throw new ArgumentException("Configuration file does not exist.");

        if (!File.Exists(hashesFilePath)) hashes = Hashes.FromFile(hashesFilePath);
        else hashes = new();

        configuration = Configuration.FromFile(configFilePath);
    }

    /// <summary>
    /// Saves the current project configuration to the appropriate files.
    /// </summary>
    internal void Save() => configuration.Save(configFilePath);

    internal void Add()
    {
        // configuration.Save();
    }

    /// <summary>
    /// Cleans the project cache
    /// </summary>
    public void Clean()
    {
        if (Directory.Exists(cacheDirectoryPath)) Directory.Delete(cacheDirectoryPath, true);
        hashes = new();
        hashes.Save(hashesFilePath);
    }

    private string[] getBeatmaps()
    {
        return Directory.GetFiles(configuration.Mapset, "*.osu");
    }
}
