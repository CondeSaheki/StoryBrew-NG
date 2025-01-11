using System.Reflection;

namespace StoryBrew.Util;

internal static partial class Helper
{
    /// <summary>
    /// Reads the contents of an embedded resource as a string.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <returns>The contents of the embedded resource.</returns>
    /// <exception cref="Exception">The resource was not found.</exception>
    public static string EmbeddedResource(string name)
    {
        const string location = "StoryBrew.Resources.";

        var assembly = Assembly.GetExecutingAssembly();

        using (Stream stream = assembly.GetManifestResourceStream(location + name) ?? throw new Exception($"Resource {name} not found."))
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}
