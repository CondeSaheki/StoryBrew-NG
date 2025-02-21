using System.Text;
using Newtonsoft.Json;
using StoryBrew.Storyboard;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Common.Beatmap;
using StoryBrew.Storyboard.Core;

namespace StoryBrew;

public partial class Bootstrap
{
    private void handleRunCommand(in string filePath)
    {
        if (!File.Exists(filePath)) throw new ArgumentException($"Project runCommand, filePath \"{filePath}\": File does not exist.");

        processProject(ProjectData.FromFile(filePath));
    }

    private Pipe.Response handleRunRequest(in string requestBody)
    {
        using Collector log = new();
        processProject(ProjectData.FromString(requestBody));    
        return new(log.Consume());
    }

    private void processProject(ProjectData project)
    {
        Log.Message("Processing project...");
        
        var name = Path.GetFileName(project.MapsetDirectoryPath) ?? "";
        var osbPath = Path.Combine(project.MapsetDirectoryPath, name + ".osb"); 
        
        StringBuilder builder = new();
        processLayers(builder, project, null);
        using var stream = new FileStream(osbPath, FileMode.Create, FileAccess.Write);
        stream.Write(Encoding.UTF8.GetBytes(builder.ToString()));

        foreach (var (path, beatmap) in getBeatmaps(project.MapsetDirectoryPath))
        {
            StringBuilder beamapBuilder = new();
            processLayers(builder, project, beatmap);
            
            Log.Warnning($"{path} Beatmap is not supported.");
            Log.Warnning(beamapBuilder.ToString());

            // using var beamapFile = new FileStream(path, FileMode.Create, FileAccess.Write);
            // beamapFile.Write(Encoding.UTF8.GetBytes(beamapBuilder.ToString()));
        }
    }

    private void processLayers(StringBuilder builder, ProjectData project, Beatmap? beatmap)
    {
        Log.Message($"Processing layers for {(beatmap == null ? "osb" : "beatmap")}");
        foreach (var layer in Enum.GetValues<Layer>()) processLayer(layer, builder, project, beatmap);
    }

    private void processLayer(Layer layer,StringBuilder builder, ProjectData project, Beatmap? beatmap)
    {
        Log.Message($"Processing layer {layer}");
        var scripts = getLayerScriptDatas(layer, project);
        foreach (var script in scripts) processScript(script, layer, builder, project, beatmap);
    }

    private void processScript(ScriptData script, Layer layer, StringBuilder builder, ProjectData project, Beatmap? beatmap)
    {
        Log.Message("Processing script...");
        var type = getScriptType(script.FullName)
            ?? throw new Exception($"Project runInstance, FullName \"{script.FullName}\": Failed to determine script type");

        Script instance;
        if (!string.IsNullOrEmpty(script.Json)) instance = JsonConvert.DeserializeObject(script.Json, type) as Script 
            ?? throw new Exception($"Failed to deserialize script \"{script.FullName}\".");
        else instance = Activator.CreateInstance(type) as Script 
            ?? throw new Exception($"Failed to create instance of script \"{script.FullName}\".");
        
        try
        {
            instance.Initialize(layer, project);
            instance.OnInitialize();
            var elements = instance.Collect(beatmap);
            foreach (var element in elements)
            {
                if (element is not Writable writer) throw new Exception($"Unhandled element type: {element.GetType()}");

                writer.Write(builder, layer, 0);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to run script {ex}");
        }
        finally
        {
            instance?.Dispose(); // Note: Dispose is not catch intentionally, exceptions in the script dispose method can not be ignored.
        }
    }

    private static List<ScriptData> getLayerScriptDatas(Layer layer, ProjectData project)
    {
        return layer switch
        {
            Layer.Video => project.Layer.Video,
            Layer.Background => project.Layer.Background,
            Layer.Fail => project.Layer.Fail,
            Layer.Pass => project.Layer.Pass,
            Layer.Foreground => project.Layer.Foreground,
            Layer.Overlay => project.Layer.Overlay,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), $"Unsupported layer: {layer}")
        };
    }

    private static (string path, Beatmap beatmap)[] getBeatmaps(in string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) return [];

        var filesPaths = Directory.GetFiles(directory, "*.osu", SearchOption.TopDirectoryOnly);
        Log.Warnning($"Beatmaps are supported, Found {filesPaths.Length} beatmaps.");
        return [];
    } 
}