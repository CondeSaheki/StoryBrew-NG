using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

using System.Text;

namespace StoryBrew.Storyboard.Element.Primitives;

public class Sample : Writable, IElement
{
    public string Path { get; }
    public double StartTime { get; }
    public int Volume { get; }

    public Sample(string path, double startTime, int volume)
    {
        Path = path;
        StartTime = startTime;
        Volume = volume;
    }

    public override string ToString() => $"Sample -> {StartTime} {Volume}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "Sample";

        var indentation = new string(' ', (int)depth);

        var result = $"{indentation}{identifier},{StartTime},{layer},\"{Path}\",{Volume}";

        writer.AppendLine(result);
    }
}
