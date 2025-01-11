using StoryBrew.Files;
using StoryBrew.Util;

namespace StoryBrew;

public partial class Project
{
    /// <summary>
    /// Adds a script with the specified name to the project configuration.
    /// </summary>
    /// <param name="name">The name of the script to add to the project configuration.</param>
    /// <exception cref="NotImplementedException">Thrown when the method is not yet implemented.</exception>
    public void Add(string name)
    {
        configuration.Save(ConfigFilePath);
        throw new NotImplementedException();
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
