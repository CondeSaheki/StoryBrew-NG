using StoryBrew.Files;

namespace StoryBrew;

public partial class Project
{
    public readonly string ProjectDirectoryPath;

    public string Name => new DirectoryInfo(ProjectDirectoryPath).Name.Trim();
    public string CacheDirectoryPath => Path.Combine(ProjectDirectoryPath, ".cache");
    public string ConfigFilePath => Path.Combine(ProjectDirectoryPath, Name + ".sbproj");
    public string BuildInfoFilePath => Path.Combine(CacheDirectoryPath, "BuildInfo");
    public string AssetsDirectoryPath => Path.Combine(ProjectDirectoryPath, "Assets"); // Maybe remove
    public string MapsetDirectoryPath => configuration.MapsetDirectoryPath;

    private ProjectConfiguration configuration;
    private BuildInfo buildInfo;

    public Project(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
        {
            throw new ArgumentException("Project path is either null, empty, or does not exist.");
        }
        ProjectDirectoryPath = projectPath;

        if (!File.Exists(ConfigFilePath)) throw new ArgumentException("Configuration file does not exist.");

        if (!File.Exists(BuildInfoFilePath)) buildInfo = new();
        else buildInfo = BuildInfo.FromFile(BuildInfoFilePath);

        configuration = ProjectConfiguration.FromFile(ConfigFilePath);
    }
}
