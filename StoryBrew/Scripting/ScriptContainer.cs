using StoryBrew.Common.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

namespace StoryBrew.Scripting
{
    public class ScriptContainer<TScript>
        where TScript : Script
    {
        private static int nextId;
        public readonly int Id = nextId++;

        private readonly ScriptManager<TScript> manager;

        public string CompiledScriptsPath { get; }

        private volatile int currentVersion = 0;
        private volatile int targetVersion = 1;

        private AssemblyLoadContext? assemblyLoadContext;
        private Type? scriptType;
        private string? scriptIdentifier;

        public string Name
        {
            get
            {
                var name = ScriptTypeName;
                if (name.Contains("."))
                    name = name.Substring(name.LastIndexOf('.') + 1);
                return name;
            }
        }

        public string ScriptTypeName { get; }
        public string MainSourcePath { get; }
        public string LibraryFolder { get; }

        public string[] SourcePaths
        {
            get
            {
                if (LibraryFolder == null || !Directory.Exists(LibraryFolder))
                    return new[] { MainSourcePath };

                return Directory.GetFiles(LibraryFolder, "*.cs", SearchOption.AllDirectories)
                    .Concat(new[] { MainSourcePath }).ToArray();
            }
        }

        private List<string> referencedAssemblies = new List<string>();
        public IEnumerable<string> ReferencedAssemblies
        {
            get { return referencedAssemblies; }
            set
            {
                var newReferencedAssemblies = new List<string>(value);
                if (newReferencedAssemblies.Count == referencedAssemblies.Count && newReferencedAssemblies.All(ass => referencedAssemblies.Contains(ass)))
                    return;

                referencedAssemblies = newReferencedAssemblies;
                ReloadScript();
            }
        }

        public bool HasScript => scriptType != null || currentVersion != targetVersion;

        // public event EventHandler OnScriptChanged;


        

        public ScriptContainer(ScriptManager<TScript> manager, string scriptTypeName, string mainSourcePath, string libraryFolder, string compiledScriptsPath, IEnumerable<string> referencedAssemblies)
        {
            this.manager = manager;
            ScriptTypeName = scriptTypeName;
            MainSourcePath = mainSourcePath;
            LibraryFolder = libraryFolder;
            CompiledScriptsPath = compiledScriptsPath;

            ReferencedAssemblies = referencedAssemblies;
        }

        public TScript CreateScript()
        {
            var localTargetVersion = targetVersion;
            if (currentVersion < localTargetVersion)
            {
                currentVersion = localTargetVersion;

                try
                {
                    if (assemblyLoadContext != null)
                    {
                        Console.WriteLine($"{nameof(Scripting)}: Unloading AssemblyLoadContext {assemblyLoadContext.Name}");
                        assemblyLoadContext.Unload();
                    }

                    var assemblyPath = Path.Combine(CompiledScriptsPath, $"{Guid.NewGuid()}.dll");
                    ScriptCompiler.Compile(SourcePaths, assemblyPath, ReferencedAssemblies);

                    var contextName = $"{Name} {Id}";
                    Console.WriteLine($"{nameof(Scripting)}: Creating AssemblyLoadContext {contextName}");
                    assemblyLoadContext = new AssemblyLoadContext(contextName, isCollectible: true);

                    try
                    {
                        scriptType = assemblyLoadContext.LoadFromAssemblyPath(assemblyPath).GetType(ScriptTypeName) ?? 
                            throw new Exception($"Type {ScriptTypeName} was not found in assembly");

                        scriptIdentifier = Guid.NewGuid().ToString();
                    }
                    catch
                    {
                        Console.WriteLine($"{nameof(Scripting)}: Unloading AssemblyLoadContext {assemblyLoadContext.Name}");
                        assemblyLoadContext.Unload();
                        throw;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"{ScriptTypeName} failed to load.\n{e}");
                }
            }

            var script = (TScript?)Activator.CreateInstance(scriptType ?? throw new Exception($"Failed to create instance of {ScriptTypeName}")) ?? throw new Exception($"Failed to create instance of {ScriptTypeName}");
            script.Identifier = scriptIdentifier;
            return script;
        }

        public void ReloadScript()
        {
            var initialTargetVersion = targetVersion;

            int localCurrentVersion;
            do
            {
                localCurrentVersion = currentVersion;
                if (targetVersion <= localCurrentVersion)
                    targetVersion = localCurrentVersion + 1;
            }
            while (currentVersion != localCurrentVersion);

        }

        public void Dispose()
        {
            if (assemblyLoadContext != null)
            {
                Console.WriteLine($"{nameof(Scripting)}: Unloading AssemblyLoadContext {assemblyLoadContext.Name}");
                assemblyLoadContext.Unload();
            }

            assemblyLoadContext = null;
            scriptType = null;
        }
    }
}
