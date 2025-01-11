using StoryBrew.Storyboarding.Commands;
using StoryBrew.Storyboarding.CommandValues;

namespace StoryBrew.Storyboarding.Display;

public class CompositeCommand<TValue> : AnimatedValue<TValue>, ITypedCommand<TValue>
    where TValue : CommandValue
{
    public CompositeCommand() : base(default(TValue) ?? throw new InvalidOperationException())
    {
    }

    public OsbEasing Easing { get { throw new InvalidOperationException(); } }
    public bool Active => true;
    public int Cost => throw new InvalidOperationException();

    public int CompareTo(ICommand? other)
        => CommandComparer.CompareCommands(this, other);


    public void WriteOsb(TextWriter writer, ExportSettings exportSettings, StoryboardTransform transform, int indentation)
    {
        throw new InvalidOperationException();
    }

    public override string ToString() => $"composite ({StartTime}s - {EndTime}s) : {StartValue} to {EndValue}";
}
