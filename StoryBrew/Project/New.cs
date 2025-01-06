namespace StoryBrew;

public partial class Project
{
    public static Project New(string path, string mapsetPath)
    {
        /*
            .vscode
            .cache
            bin
            obj
            AssetsLibrary
            ScriptsLibrary

            .gitignore
            name.csproj
            name.sbproj
            name.sln
        */

        var name = new DirectoryInfo(path).Name;

        var slnPath = Path.Combine(path, name + ".sln");
        var csprojPath = Path.Combine(path, name + ".csproj");
        var gitignorePath = Path.Combine(path, name + ".gitignore");

        var vsCodePath = Path.Combine(path, ".vscode");
        var cachePath = Path.Combine(path, ".cache");
        var assetsLibrary = Path.Combine(path, "AssetsLibrary");
        var scriptsLibrary = Path.Combine(path, "ScriptsLibrary");

        Directory.CreateDirectory(path);
        Directory.CreateDirectory(vsCodePath);
        Directory.CreateDirectory(cachePath);
        Directory.CreateDirectory(assetsLibrary);
        Directory.CreateDirectory(scriptsLibrary);

        File.Copy("", slnPath, overwrite: true);
        File.Copy("", csprojPath, overwrite: true);

        string gitignore = @"
        bin/
        obj/

        .vs/
        .idea/

        # StoryBrew cache
        .cache
        ";

        File.WriteAllText(gitignorePath, gitignore);

        var project = new Project(path);
        project.configuration.Mapset = mapsetPath;

        return project;
    }
}
