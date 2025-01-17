using System.Reflection;
using System.Text;
using StoryBrew.Project.Files;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;

namespace StoryBrew.Project;

public partial class Manager
{
    /// <summary>
    /// Runs all the scripts in the project. If a script fails it will be skipped.
    /// </summary>
    /// <param name="log">The log of the build process.</param>
    /// <returns><c>true</c> if the run was successful, <c>false</c> otherwise.</returns>
    public bool Run(out string log)
    {
        StringBuilder logBuilder = new();

        var buildStatus = Build(out var buildLog);
        logBuilder.AppendLine("Build:");
        logBuilder.AppendLine(buildLog);

        if (!buildStatus)
        {
            log = logBuilder.ToString();
            return false;
        }

        string assemblyFilePath = Path.Combine(CacheDirectoryPath, Name + ".dll");
        var assembly = Assembly.Load(File.ReadAllBytes(assemblyFilePath));
        var scriptTypes = assembly.GetTypes()
                              .Where(type => typeof(Script).IsAssignableFrom(type))
                              .ToDictionary(type => type.FullName ?? throw new Exception("Unable to get script FullName."));

        var beatmaps = getBeatmaps();

        void runLayer(Layer layer, List<ScriptConfiguration> scripts)
        {
            foreach (var script in scripts)
            {
                try
                {
                    if (!scriptTypes.TryGetValue(script.FullName, out var type)) throw new Exception($"Script {script.FullName} not found.");

                    object instance = Activator.CreateInstance(type) ?? throw new Exception($"Failed to create instantiate for {script.FullName}.");
                    {

                    }
                    if (instance is Script scriptInstance)
                    {
                        try
                        {
                            scriptInstance.Init(layer, this);


                            foreach (var configurable in script.Configurables)
                            {
                                if (configurable.Default == configurable.Value) continue;

                                var field = type.GetField(configurable.Name, BindingFlags.Public | BindingFlags.Instance)
                                    ?? throw new Exception($"Failed to find field {configurable.Name} in {type.FullName}");

                                if (field.GetType() != configurable.Type)
                                    throw new Exception($"Field {configurable.Name} in {type.FullName} is not of type {configurable.Type}");

                                field.SetValue(instance, configurable.Value);
                            }

                            var osb = scriptInstance.Collect();

                            foreach (var beatmap in beatmaps)
                            {
                                var osu = scriptInstance.Collect();
                            }

                            // TODO: process osb and osu

                        }
                        finally
                        {
                            scriptInstance?.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {

                    logBuilder.AppendLine($"Failed to run {script.FullName}: {ex}");
                }
            }
        }

        logBuilder.AppendLine($"Running Background scripts");
        runLayer(Layer.Background, configuration.Background);
        logBuilder.AppendLine($"Running Fail scripts");
        runLayer(Layer.Fail, configuration.Fail);
        logBuilder.AppendLine($"Running Pass scripts");
        runLayer(Layer.Pass, configuration.Pass);
        logBuilder.AppendLine($"Running Foreground scripts");
        runLayer(Layer.Foreground, configuration.Foreground);
        logBuilder.AppendLine($"Running Overlay scripts");
        runLayer(Layer.Overlay, configuration.Overlay);

        // TODO: add video layer

        // logBuilder.AppendLine($"Running Video scripts");
        // runLayer(OsbLayer.Video, configuration.Video);

        log = logBuilder.ToString();
        return true;
    }

    private object[] getBeatmaps()
    {
        return Directory.GetFiles(configuration.MapsetDirectoryPath, "*.osu").Select(file => file).ToArray();
    }
}
