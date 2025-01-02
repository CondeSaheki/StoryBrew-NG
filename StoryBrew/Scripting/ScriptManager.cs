using StoryBrew.Common.Scripting;
using System;
using System.Collections.Generic;
using System.IO;

namespace StoryBrew.Scripting
{
    public class ScriptManager<TScript> : IDisposable where TScript : Script
    {
        private readonly string scriptsNamespace;
        private readonly string? commonScriptsPath;
        private readonly string scriptsLibraryPath;
        private readonly string compiledScriptsPath;

        private Dictionary<string, ScriptContainer<TScript>> scriptContainers = [];
        private List<string> referencedAssemblies = [];

        public IEnumerable<string> ReferencedAssemblies
        {
            get { return referencedAssemblies; }
            set
            {
                referencedAssemblies = new List<string>(value);
                foreach (var scriptContainer in scriptContainers.Values)
                    scriptContainer.ReferencedAssemblies = referencedAssemblies;
                UpdateSolutionFiles();
            }
        }

        public string ScriptsPath { get; }

        public ScriptManager(string scriptsNamespace, string scriptsSourcePath, string? commonScriptsPath, string scriptsLibraryPath, string compiledScriptsPath, IEnumerable<string> referencedAssemblies)
        {
            this.scriptsNamespace = scriptsNamespace;
            ScriptsPath = scriptsSourcePath;
            this.commonScriptsPath = commonScriptsPath;
            this.scriptsLibraryPath = scriptsLibraryPath;
            this.compiledScriptsPath = compiledScriptsPath;

            ReferencedAssemblies = referencedAssemblies;
        }

        public ScriptContainer<TScript> Get(string scriptName)
        {
            if (scriptContainers.TryGetValue(scriptName, out ScriptContainer<TScript>? scriptContainer))
                return scriptContainer;

            var scriptTypeName = $"{scriptsNamespace}.{scriptName}";
            var sourcePath = Path.Combine(ScriptsPath, $"{scriptName}.cs");

            if (commonScriptsPath != null && !File.Exists(sourcePath))
            {
                var commonSourcePath = Path.Combine(commonScriptsPath, $"{scriptName}.cs");
                if (File.Exists(commonSourcePath))
                {
                    File.Copy(commonSourcePath, sourcePath);
                    File.SetAttributes(sourcePath, File.GetAttributes(sourcePath) & ~FileAttributes.ReadOnly);
                }
            }

            scriptContainer = new ScriptContainer<TScript>(this, scriptTypeName, sourcePath, scriptsLibraryPath, compiledScriptsPath, referencedAssemblies);
            scriptContainers.Add(scriptName, scriptContainer);
            return scriptContainer;
        }

        public IEnumerable<string> GetScriptNames()
        {
            var projectScriptNames = new List<string>();
            foreach (var scriptPath in Directory.GetFiles(ScriptsPath, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileNameWithoutExtension(scriptPath);
                projectScriptNames.Add(name);
                yield return name;
            }
            if (commonScriptsPath != null)
            {
                foreach (var scriptPath in Directory.GetFiles(commonScriptsPath, "*.cs", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileNameWithoutExtension(scriptPath);
                    if (!projectScriptNames.Contains(name))
                        yield return name;
                }
            }
        }

        public void UpdateSolutionFiles()
        {
            Console.WriteLine("Updating solution files");

            var sourceSlnPath = Path.Combine("res", "storyboard.sln");
            var sourceCsProjPath = Path.Combine("res", "scripts.csproj");
            if (!File.Exists(sourceSlnPath)) throw new Exception("Missing res");
            if (!File.Exists(sourceCsProjPath)) throw new Exception("Missing res");

            var slnPath = Path.Combine(ScriptsPath, "storyboard.sln");
            var csProjPath = Path.Combine(ScriptsPath, "scripts.csproj");

            File.Copy(sourceSlnPath, slnPath, overwrite: true);
            File.Copy(sourceCsProjPath, csProjPath, overwrite: true);

            var vsCodePath = Path.Combine(ScriptsPath, ".vscode");
            if (!Directory.Exists(vsCodePath)) Directory.CreateDirectory(vsCodePath);
        }

        public void Dispose()
        {
            foreach (var container in scriptContainers.Values) container.Dispose();
            scriptContainers.Clear();
        }
    }
}
