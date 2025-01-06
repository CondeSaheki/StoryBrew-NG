using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace StoryBrew;

public partial class Project
{
    public void Build()
    {
        foreach (var script in configuration.Instances.Values.Select(instance => instance.ScriptFile).Distinct())
        {
            Build(Path.Combine(ProjectPath, script));
        }
    }

    public void Build(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        var assemblyfile = Path.Combine(cacheDirectoryPath, name + ".dll");

        string? hash = null;
        if (File.Exists(assemblyfile))
        {
            if (hashes.ScriptsFiles.TryGetValue(filePath, out var hashValue))
            {
                hash = Hashes.FileHash(filePath);

                if (hashValue == hash) return; // file exist and hash match
                File.Delete(assemblyfile);
            }
        }

        _ = hashes.ScriptsFiles.Remove(filePath); // safe remove hash

        var libraryPaths = Directory.GetFiles(ScriptsLibraryPath, "*.cs", SearchOption.AllDirectories);

        if (libraryPaths.Length != hashes.ScriptsLibrary.Count)
        {
            hashes.ScriptsLibrary = Hashes.FileHashes(libraryPaths);
        };

        // compile

        var sorceCodePaths = new string[libraryPaths.Length + 1];
        libraryPaths[..].CopyTo(sorceCodePaths, 0);
        sorceCodePaths[^1] = filePath;

        var references = getAssemblyReferences();

        build(name, cacheDirectoryPath, sorceCodePaths, references);

        // save hashes

        hashes.ScriptsFiles[filePath] = hash ?? Hashes.FileHash(filePath);
        hashes.Save(cacheDirectoryPath);
    }

    private static void build(string name, string outputPath, string[] sourceCodePaths, string[] assemblyReferences)
    {
        var outputFile = Path.Combine(outputPath, name + ".dll");

        // syntax trees

        var syntaxTrees = sourceCodePaths.Select(codePath =>
        {
            var raw = File.ReadAllText(codePath);
            return CSharpSyntaxTree.ParseText(raw);
        });

        // references

        MetadataReference[] references = new MetadataReference[assemblyReferences.Length + 1];

        for (int i = 0; i < assemblyReferences.Length; i++)
        {
            string reference = assemblyReferences[i];
            if (!File.Exists(reference)) throw new ArgumentException($"Assembly reference does not exist: {reference}");

            references[i] = MetadataReference.CreateFromFile(reference);
        }

        references[^1] = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        // compile

        var compilation = CSharpCompilation.Create(
            name,
            syntaxTrees,
            references,
            new(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);

        // result

        StringBuilder sb = new();
        foreach (var diagnostic in result.Diagnostics) sb.AppendLine(diagnostic.ToString());

        Console.WriteLine(sb.ToString());

        if (!result.Success) throw new Exception("Compilation failed.");

        // check

        var assembly = Assembly.Load(stream.ToArray());
        if (assembly.GetType(name) == null) throw new Exception($"Class {name} do not exists in assembly.");

        // output

        using (var fileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
        stream.WriteTo(fileStream);
    }

    private string[] getAssemblyReferences()
    {
        var app = AppDomain.CurrentDomain.BaseDirectory;
        string[] defaultAssemblies =
        [
            Path.Combine(app, "StroryBrew.Common.dll"),
            Path.Combine(app, "SkiaSharp.dll"),
            Path.Combine(app, "OpenTk.Mathematics.dll"),
        ];

        var projectAssemblies = Directory.GetFiles(ProjectPath, "*.dll");

        var resultArray = new string[defaultAssemblies.Length + projectAssemblies.Length];
        Array.Copy(defaultAssemblies, resultArray, defaultAssemblies.Length);
        Array.Copy(projectAssemblies, 0, resultArray, defaultAssemblies.Length, projectAssemblies.Length);

        return resultArray;
    }
}
