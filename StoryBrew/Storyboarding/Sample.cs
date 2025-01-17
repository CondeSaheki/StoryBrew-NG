using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public class Sample : IStoryboardElement
{
    public string Path { get; set; } = string.Empty;
    public double Time { get; set; } = 0;
    public double Volume { get; set; } = 100;
    public double StartTime => Time;
    public double EndTime => Time;

    public void WriteOsb(TextWriter writer, ExportSettings exportSettings, Layer layer, StoryboardTransform? transform)
        => writer.WriteLine($"Sample,{((int)Time).ToString(exportSettings.NumberFormat)},{layer},\"{Path.Trim()}\",{((int)Volume).ToString(exportSettings.NumberFormat)}");
}
