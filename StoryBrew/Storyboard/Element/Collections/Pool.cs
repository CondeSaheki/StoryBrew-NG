using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Element.Collections;

public class Pool<T> : Writable, IElement where T : ElementTransformable, IElement
{
    public readonly Func<T> ElementFactory;

    private List<PoolElement> commands = [];
    private bool compoundCommands;

    private readonly Lock commadsLock = new();

    public Pool(Func<T> elementFactory, bool compoundCommands = false)
    {
        ElementFactory = elementFactory;
        this.compoundCommands = compoundCommands;
    }

    public PoolElement Get()
    {
        lock (commadsLock)
        {
            PoolElement element = new(compoundCommands);
            commands.Add(element);
            return element;
        }
    }

    public void Get(out PoolElement value) => value = Get();

    public Segment ToSegment() => new(combine());

    public Container ToContainer()
    {
        if (compoundCommands) throw new InvalidOperationException("Cannot convert to container because allowCompound is true");
        return new(combine());
    }

    private List<T> combine()
    {
        lock (commadsLock)
        {
            if (commands.Count == 0) return [];

            List<T> elements = [];

            foreach (var pool in commands)
            {
                bool reused = false;
                foreach (var element in elements)
                {
                    if (element.StartTime < pool.EndTime && element.EndTime > pool.StartTime) continue; // overlaps

                    reused = true;
                    foreach (var command in pool.Commands) element.Transform(command);

                    break;
                }
                if (reused) continue;

                var instance = ElementFactory.Invoke();

                foreach (var command in pool.Commands) instance.Transform(command);

                elements.Add(instance);
            }
            return elements;
        }
    }

    public override string ToString() => $"ElementPool -> {commands.Count}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0) => ToSegment().Write(writer, layer, depth);
}

public class PoolElement : Transformable
{
    internal PoolElement(bool allowCompound = false) : base(allowCompound)
    {
    }

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0) => throw new NotSupportedException("Write is not supported for PoolElement.");
}
