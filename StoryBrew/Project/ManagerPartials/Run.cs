using System.Reflection;
using System.Text;
using StoryBrew.Mapset;
using StoryBrew.Project.Files;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;

namespace StoryBrew.Project;

public partial class Manager
{
    /// <summary>
    /// Attempts to run all the scripts in the project. If a script fails it will be skipped.
    /// </summary>
    /// <param name="log">The log of the run process.</param>
    /// <returns><c>true</c> if the run was successful, <c>false</c> otherwise.</returns>
    public bool TryRun(out string log) => Run(out log) != null;

    /// <summary>
    /// Runs all the scripts in the project. If a script fails it will be skipped.
    /// </summary>
    /// <param name="log">The log of the build and run process.</param>
    /// <returns>The content of the .osb file if the run was successful, <c>null</c> otherwise.</returns>
    public string? Run(out string log)
    {
        StringBuilder logBuilder = new();

        var assembly = Build(out var buildLog);
        logBuilder.AppendLine("Build:");
        logBuilder.AppendLine(buildLog);

        if (assembly == null)
        {
            log = logBuilder.ToString();
            return null;
        }

        // var beatmaps = getBeatmaps();
        var scriptInfos = getScriptInfos(assembly);

        var osbContent = runGroup(logBuilder, scriptInfos);
        //  var osuContents = beatmaps.Select(tuple => (tuple.filePath, tuple.beatmap, runGroup(logBuilder, scriptInfos, tuple.beatmap))).ToArray();

        var filePath = MapsetDirectoryPath == string.Empty ? Path.Combine(MapsetDirectoryPath, $"{Name}.osb") : Path.Combine(ProjectDirectoryPath, $"{Name}.osb");
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var StreamWriter = new StreamWriter(fileStream);
        StreamWriter.Write(osbContent);

        log = logBuilder.ToString();

        return osbContent;
    }

    private (Layer, ScriptConfiguration, Type)[] getScriptInfos(Assembly assembly)
    {
        var types = assembly.GetTypes()
                            .Where(type => typeof(Script).IsAssignableFrom(type))
                            .ToDictionary(type => type.FullName ?? throw new Exception("Unable to get type name."), type => type);

        return
        [
            .. configuration.Video.Select(configuration => (Layer.Video, configuration, types[configuration.FullName])),
            .. configuration.Background.Select(configuration => (Layer.Background, configuration, types[configuration.FullName])),
            .. configuration.Fail.Select(configuration => (Layer.Fail, configuration, types[configuration.FullName])),
            .. configuration.Pass.Select(configuration => (Layer.Pass, configuration, types[configuration.FullName])),
            .. configuration.Foreground.Select(configuration => (Layer.Foreground, configuration, types[configuration.FullName])),
            .. configuration.Overlay.Select(configuration => (Layer.Overlay, configuration, types[configuration.FullName])),
        ];
    }

    private string runGroup(StringBuilder logBuilder, (Layer, ScriptConfiguration, Type)[] scriptInfo, Beatmap? beatmap = null)
    {
        StringBuilder builder = new();

        logBuilder.AppendLine(beatmap == null ? "Running Group" : $"Running Group -> {beatmap}");
        foreach (var (layer, config, type) in scriptInfo) builder.Append(runTarget(logBuilder, layer, config, type, beatmap));

        return builder.ToString();
    }

    private string runTarget(StringBuilder logBuilder, Layer layer, ScriptConfiguration configuration, Type type, Beatmap? beatmap = null)
    {
        logBuilder.AppendLine($"Running -> {layer} -> {configuration.FullName}");

        Script? script = null;
        StringBuilder builder = new();
        try
        {
            script = createInstance(type, configuration, layer);

            var elements = beatmap == null ? script.Collect() : script.Collect(beatmap);

            foreach (var element in elements)
            {
                if (element is not Writable writable) throw new InvalidOperationException($"Unhandled element type: {element.GetType()}");
                writable.Write(logBuilder, builder, layer);
            }
        }
        catch (Exception ex)
        {
            logBuilder.AppendLine($"Failed to run {configuration.FullName}: {ex}");
        }

        script?.Dispose(); // this is not catch here intentionally, exception in disposal should not be ignored.

        return builder.ToString();
    }

    private Script createInstance(Type type, ScriptConfiguration configuration, Layer layer)
    {
        var obj = Activator.CreateInstance(type) ?? throw new Exception($"Failed to create instantiate for {configuration.FullName}.");
        if (obj is not Script script) throw new Exception("");

        script.Init(layer, this);

        foreach (var configurable in configuration.Configurables)
        {
            if (configurable.Default == configurable.Value) continue;

            var field = type.GetField(configurable.Name, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new Exception($"Failed to find field {configurable.Name} in {type.FullName}");

            if (field.FieldType != configurable.Type)
                throw new Exception($"Field {configurable.Name} in {type.FullName} is not of type {configurable.Type} but {field.FieldType}");

            field.SetValue(script, Convert.ChangeType(configurable.Value, field.FieldType));
        }

        return script;
    }

    private (string filePath, Beatmap beatmap)[] getBeatmaps()
    {
        throw new NotImplementedException("Beatmaps are not supported currently");

        // var beatmapFilePaths = Directory.GetFiles(configuration.MapsetDirectoryPath, "*.osu").Select(file => file);
        // var beatmps = beatmapFilePaths.Select(filePath => new Beatmap(filePath));
    }
}
