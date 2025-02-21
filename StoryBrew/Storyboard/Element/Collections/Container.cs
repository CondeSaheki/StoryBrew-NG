using System.Text;
using OpenTK.Mathematics;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Element.Collections;

public class Container : Transformable, IElement
{
    public Anchor Origin;
    public Vector2 InitialPosition;
    public IReadOnlyList<ElementTransformable> Elements => elements;

    private readonly List<ElementTransformable> elements = [];

    public Container(IEnumerable<ElementTransformable> elements, Anchor origin = Anchor.Centre) : base(false)
    {
        if (elements.Any(element => element.AllowCompound && (element.Triggers!.Count != 0 || element.Loops!.Count != 0)))
            throw new ArgumentException("Elements with triggers or loops can not be added to containers.");

        this.elements = [.. elements];
        foreach (var element in elements) element.AllowCompound = false;
        Origin = origin;
    }
    public Container(Anchor origin = Anchor.Centre) : base(false)
    {
        Origin = origin;
    }

    public void Add(ElementTransformable element, out ElementTransformable value)
    {
        value = element;
        Add(element);
    }

    public void Add(ElementTransformable element)
    {
        element.AllowCompound = false;
        elements.Add(element);
    }

    public override string ToString() => $"Container: ";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        foreach (var element in Elements)
        {
            if (!element.HasCommands) throw new InvalidOperationException("Cannot write element with no commands");
            element.Write(writer, layer, depth);
        }
    }
}
