using System.Text;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding;

public class Container : Group, IElement
{
    public Anchor Origin;
    public Vector2 InitialPosition;
    public IReadOnlyList<Commandable> Elements => elements;

    private readonly List<Commandable> elements = [];

    public Container(IEnumerable<Commandable> elements, Anchor origin = Anchor.Centre) : base(false)
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

    public void Add(Commandable element, out Commandable value)
    {
        value = element;
        Add(element);
    }

    public void Add(Commandable element)
    {
        element.AllowCompound = false;
        elements.Add(element);
    }

    public override string ToString() => $"Container: ";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        foreach (var element in Elements)
        {
            if (!element.HasCommands) throw new InvalidOperationException("Cannot write element with no commands");
            element.Write(log, writer, layer, depth);
        }
    }
}
