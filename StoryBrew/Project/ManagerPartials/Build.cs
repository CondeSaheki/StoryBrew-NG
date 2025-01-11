using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StoryBrew.Util;

namespace StoryBrew.Project;

public partial class Manager
{
    /// <summary>
    /// Builds the project into a single assembly and updates the build info.
    /// The project is only built if the source code files have changed since the last build.
    /// </summary>
    /// <param name="log">The log of the build process.</param>
    /// <returns><c>true</c> if the build was successful, <c>false</c> otherwise.</returns>
    public bool Build(out string log)
    {
        /*
            Note: Unlike the old StoryBrew, this compiler do not watch the source code files for changes atively, instead it only checks
            if the source code files have changed since the last build using hashes making it less dependent on file system IO operations.

            Note: All .cs Files in the project directory and its subdirectories are compiled into a single assembly,
            it allows for more freedom and better quality of Life in the script creation process.

            Note: Adds all .dll files in the project directory to the references list simplyfying the process to add new dependencies to the project.

            Note: The source code files are getting read twice in this function maybe stream can be reused, not sure how to do
            syntax trees creation with streams

            Note: is recomended to use AssemblyMetadata instead of MetadataReference, attempted to use but gave an error that i did not understod.
        */

        // files, source codes

        string assemblyFilePath = Path.Combine(CacheDirectoryPath, Name + ".dll");
        var sourceCodeFilePaths = Directory.EnumerateFiles(ProjectDirectoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                           !file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .ToArray();

        if(sourceCodeFilePaths.Length == 0)
        {
            log = "No source code files found in the project directory.";
            return false;
        };

        // hash check

        if (matchFiles(buildInfo.Hashes, sourceCodeFilePaths, out var currentHashes) && File.Exists(assemblyFilePath))
        {
            log = "The project has not changed since the last build.";
            return true;
        }

        buildInfo.Hashes = [];
        if (File.Exists(BuildInfoFilePath)) File.Delete(BuildInfoFilePath);
        if (File.Exists(assemblyFilePath)) File.Delete(assemblyFilePath);

        // syntax trees

        var syntaxTrees = sourceCodeFilePaths.Select(codePath =>
        {
            var raw = File.ReadAllText(codePath);
            return CSharpSyntaxTree.ParseText(raw);
        });

        // references

        var app = AppDomain.CurrentDomain.BaseDirectory;
        string[] defaultReferences =
        [
            Path.Combine(app, "StoryBrew.dll"),
            Path.Combine(app, "SkiaSharp.dll"),
            Path.Combine(app, "OpenTK.Mathematics.dll"),
        ];

        var runtimeReferences = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
        var projectReferences = Directory.GetFiles(ProjectDirectoryPath, "*.dll");

        var assemblyReferenceFilePaths = defaultReferences.Concat(projectReferences).Concat(runtimeReferences).ToArray();

        /*
            AssemblyMetadata.CreateFromStream(fileStream);
            assemblyMetadatas.Select(metadata => metadata.GetReference()),
            finally { foreach (var reference in assemblyMetadatas) reference?.Dispose(); }
        */

        var assemblyMetadatas = assemblyReferenceFilePaths.Select(file =>
        {
            if (!File.Exists(file)) throw new ArgumentException($"Assembly reference does not exist: {file}");

            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            return MetadataReference.CreateFromStream(fileStream);
        }).ToArray();

        // compile

        var compilation = CSharpCompilation.Create(
            Name,
            syntaxTrees,
            assemblyMetadatas,
            new(OutputKind.DynamicallyLinkedLibrary));

        using var memoryStream = new MemoryStream();
        var result = compilation.Emit(memoryStream);

        // output

        StringBuilder diagnosticBuilder = new();
        foreach (var diagnostic in result.Diagnostics) diagnosticBuilder.AppendLine(diagnostic.ToString());
        log = diagnosticBuilder.ToString();

        if (!result.Success) return false;

        var assembly = Assembly.Load(memoryStream.ToArray());
        var info = scriptInfo(assembly);
        if(info.Count == 0)
        {
            log = "No scripts found in the project.";
            return false;
        }

        using (var fileStream = new FileStream(assemblyFilePath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.WriteTo(fileStream);
        }

        buildInfo.ScriptsInfo = info;
        buildInfo.Hashes = currentHashes;
        buildInfo.Save(BuildInfoFilePath, true);

        return true;
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
/*

    // Workspace MSBuild alternative needs more researh and testing

 using Microsoft.Build.Locator;
 using Microsoft.CodeAnalysis.MSBuild;

    private static bool build()
    {
        MSBuildLocator.RegisterDefaults();

        using var workspace = MSBuildWorkspace.Create();

        if (!File.Exists(projectPath)) return false;
        var project = await workspace.OpenProjectAsync(projectPath);

        var compilation = await project.GetCompilationAsync();
        if (compilation == null) return false;

        var result = compilation.Emit("output.dll");

        return result.Success;
    }
*/
}
