using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

using System.Text;

namespace StoryBrew.Storyboard.Element.Primitives;

public class Raw : Writable, IElement
{
    public string Content { get; set; }

    public Action<StringBuilder, Layer, uint> Writer { get; set; }

    public Raw(string? content = null, Action<StringBuilder, Layer, uint>? action = null) 
    {
        Content = content ?? string.Empty;
        Writer = action ?? ((writer, layer, depth) => writer.AppendLine(content));
    }

    public override string ToString() => $"Raw -> {Content}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0) => Writer.Invoke(writer, layer, depth);
}
