using StoryBrew.Project.Files;
using StoryBrew.Scripting;
using StoryBrew.Util;

namespace StoryBrew.Project;

public partial class Manager
{
    /// <summary>
    /// Adds a script with the specified name to the project configuration.
    /// </summary>
    /// <param name="name">The name of the script to add to the project configuration.</param>
    /// <exception cref="NotImplementedException">Thrown when the method is not yet implemented.</exception>
    public void Add(string name)
    {
        if (!buildInfo.ScriptsInfo.TryGetValue(name, out var infos)) throw new ArgumentException("Script does not exist.");

        configuration.Background.Add(new ScriptConfiguration
        {
            // Nickname = name,
            FullName = name,
            Configurables = [.. infos.Select(info => new Configurable(info))],
        });

        configuration.Save(ConfigFilePath, true);
    }

    /// <summary>
    /// Cleans the project cache
    /// </summary>
    public void Clean()
    {
        if (Directory.Exists(CacheDirectoryPath)) Directory.Delete(CacheDirectoryPath, true);
        buildInfo = new();
    }

    /// <summary>
    /// Creates a new script file with the specified name in the project directory.
    /// </summary>
    /// <param name="name">The name of the script file to create.</param>
    public void Create(string name)
    {
        var scriptTemplate = Helper.EmbeddedResource("cs");
        scriptTemplate = scriptTemplate.Replace("{name}", name);

        File.WriteAllText(Path.Combine(ProjectDirectoryPath, name + ".cs"), scriptTemplate);
    }
}
