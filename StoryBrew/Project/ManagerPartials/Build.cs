/*
    Note: Regards to the build process,
    Roslyn (Microsoft.CodeAnalysis.CSharp) is powerful and highly performant, it allows in-memory output minimizing I/O operations. 
    However, adding references is not flexible because it does not read .csproj files, making it difficult to add dependencies like SkiaSharp, which includes native runtimes.
    Roslyn Workspaces (Microsoft.CodeAnalysis.Workspaces.MSBuild) addresses this limitation by reading .csproj files and correctly resolving references. 
    However, resolving references is slow and requires caching. Additionally, it does not restore dependencies, meaning a custom mechanism is needed 
    to restoring dependencies and NuGet packages. Implementing this is complex and may introduce other issues.
    MSBuild (Microsoft.Build) solves these problems, but it does not support in-memory compilation, which is one of the main reasons for implementing a custom compiler.
    Considering all these factors, I concluded that the simplest and most reliable approach despite lacking in-memory output is to invoke dotnet build as an external process.
*/

using System.Diagnostics;
using System.Reflection;
using StoryBrew.Scripting;
using StoryBrew.Util;

namespace StoryBrew.Project;

public partial class Manager
{
    /// <summary>
    /// Attempts to build the project.
    /// </summary>
    /// <returns><c>true</c> if the build was successful, <c>false</c> otherwise.</returns>
    public bool TryBuild()
    {
        try
        {
            return Build() != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to build project: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Builds the project into a single assembly and updates the build info.
    /// </summary>
    /// <returns> the assembly if the build was successful, <c>null</c> otherwise.</returns>
    public Assembly? Build()
    {
        Console.WriteLine($"Building project {Name}:");

        var projectfilePath = Path.Combine(ProjectDirectoryPath, $"{Name}.csproj");
        var assemblyFilePath = Path.Combine(ProjectDirectoryPath, "bin", "Release", "net8.0", Name + ".dll");

        var sucess = buildProcess(projectfilePath);

        if (!sucess) return null;
        
        if (!File.Exists(assemblyFilePath))
        {
            Console.WriteLine($"Assembly file not found at {assemblyFilePath}.");
            return null;
        }

        var assembly = Assembly.LoadFile(assemblyFilePath);

        var info = scriptInfo(assembly);
        if (info.Count == 0)
        {
            Console.WriteLine("No scripts found in the project.");
            return null;
        }

        Directory.CreateDirectory(CacheDirectoryPath);

        buildInfo.ScriptsInfo = info;
        buildInfo.Hashes = []; //Helper.SHA256File(assemblyFilePath)
        buildInfo.Save(BuildInfoFilePath, true);

        return assembly;
    }

    private static bool buildProcess(string projectDirectoryPath)
    {
        var config = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build -c Release {projectDirectoryPath}",
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = config };
        process.Start();
        process.WaitForExit();

        return process.ExitCode == 0;
    }

    private static Dictionary<string, List<ConfigurableInfo>> scriptInfo(Assembly assembly)
    {
        var scripts = assembly.GetTypes().Where(type => typeof(Script).IsAssignableFrom(type)).ToArray();

        Dictionary<string, List<ConfigurableInfo>> infos = new(scripts.Length);

        foreach (var script in scripts)
        {
            var id = script.FullName ?? throw new Exception("Unable to get script FullName.");
            List<ConfigurableInfo> configurables = [];

            var configurableFields = script.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => Attribute.IsDefined(field, typeof(ConfigurableAttribute)));

            if (configurableFields.Any())
            {
                // Note: Configurable Attribute could have an argument "Default" to avoid the need for a instantiation here
                // but to make it simpler made it this way

                var instance = Activator.CreateInstance(script) ?? throw new Exception($"Failed to instantiate {id}.");
                foreach (var field in configurableFields) configurables.Add(new(field, instance));
            }

            infos.Add(id, configurables);
        }
        return infos;
    }

    private static bool matchFiles(HashSet<string> hashes, string[] filePaths, out HashSet<string> currentHashes)
    {
        currentHashes = new(filePaths.Length);
        foreach (var filePath in filePaths) currentHashes.Add(Helper.SHA256File(filePath));

        return currentHashes.SetEquals(hashes);
    }

    private static IEnumerable<string> enumerateFiles(string directoryPath, HashSet<string> ignoreDirectories, HashSet<string> extensions)
    {
        if (ignoreDirectories.Count == 0) throw new ArgumentException("ignoreDirectories cannot be empty.", nameof(ignoreDirectories));
        var directories = Directory.EnumerateDirectories(directoryPath)
            .Where(directory => !ignoreDirectories.Contains(Path.GetFileName(directory)));

        return process(directoryPath, extensions, false).Concat(directories.SelectMany(directory => process(directory, extensions, true)));

        static IEnumerable<string> process(string directoryPath, HashSet<string> extensions, bool recursive)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = recursive,
                AttributesToSkip = FileAttributes.System | FileAttributes.Hidden,
                BufferSize = 4096,
                IgnoreInaccessible = true
            };

            return extensions.Count switch
            {
                0 => Directory.EnumerateFiles(directoryPath, "*", options),
                1 => Directory.EnumerateFiles(directoryPath, $"*.{extensions.First()}", options),
                _ => Directory.EnumerateFiles(directoryPath, "*", options)
                              .Where(file => extensions.Contains(Path.GetExtension(file).TrimStart('.'))),
            };
        }
    }
}