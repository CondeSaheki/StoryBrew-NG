using StoryBrew.Files;
using StoryBrew.Util;

namespace StoryBrew;

public partial class Project
{
    /// <summary>
    /// Creates a new project at the specified path with the necessary directories and files.
    /// </summary>
    /// <param name="path">The file system path where the project will be created.</param>
    /// <param name="mapsetPath">The path to the mapset directory.</param>
    /// <returns>A new instance of the Project class initialized with the specified path.</returns>
    public static Project New(string path, string mapsetPath)
    {
        /*
            .
            ├── Assets
            ├── bin
            ├── obj
            ├── .gitignore
            ├── name.csproj
            ├── name.sbproj
            └── name.sln
        */

        var name = new DirectoryInfo(path).Name.Trim();

        var slnFilePath = Path.Combine(path, name + ".sln");
        var csprojFilePath = Path.Combine(path, name + ".csproj");
        var gitignoreFilePath = Path.Combine(path, ".gitignore");
        var configurationFilePath = Path.Combine(path, name + ".sbproj");

        var assetsDirectoryPath = Path.Combine(path, "Assets");

        Directory.CreateDirectory(assetsDirectoryPath);

        string sln = Helper.EmbeddedResource("sln");
        string gitignore = Helper.EmbeddedResource("gitignore");
        string csproj = Helper.EmbeddedResource("csproj");

        File.WriteAllText(slnFilePath, sln);
        File.WriteAllText(gitignoreFilePath, gitignore);
        File.WriteAllText(csprojFilePath, csproj);

        ProjectConfiguration config = new()
        {
            MapsetDirectoryPath = mapsetPath
        };

        config.Save(configurationFilePath, true);

        return new Project(path);
    }
}
