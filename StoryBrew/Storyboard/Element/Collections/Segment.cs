using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Element.Collections;

public class Segment : Writable, IElement
{
    public IReadOnlyList<IElement> Elements => elements;

    private List<IElement> elements = [];

    public Segment(IEnumerable<IElement> elements)
    {
        this.elements = [.. elements];
    }

    public void Add<T>(T element) where T : IElement => elements.Add(element);

    public void Add<T>(T element, out T value) where T : IElement
    {
        Add(element);
        value = element;
    }

    public override string ToString() => $"Segment: ";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        foreach (var element in Elements)
        {
            if (element is not Writable writable) throw new InvalidOperationException($"Unhandled element type: {element.GetType()}");
            writable.Write(writer, layer, depth);
        }
    }
}
