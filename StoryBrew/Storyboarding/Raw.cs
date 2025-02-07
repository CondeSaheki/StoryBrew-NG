using System.Text;

namespace StoryBrew.Storyboarding;

public class Raw : Writable, IElement
{
    public string Content { get; set; }

    public Action<StringBuilder, StringBuilder, Layer, uint> Writer { get; set; }

    public Raw(string? content = null, Action<StringBuilder, StringBuilder, Layer, uint>? action = null) 
    {
        Content = content ?? string.Empty;
        Writer = action ?? ((log, writer, layer, depth) => writer.AppendLine(content));
    }

    public override string ToString() => $"Raw -> {Content}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0) => Writer.Invoke(log, writer, layer, depth);
}
