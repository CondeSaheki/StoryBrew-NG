using StoryBrew.Storyboarding.Commands;
using StoryBrew.Storyboarding.CommandValues;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.Display;

public class CompositeCommand<TValue> : AnimatedValue<TValue>, ITypedCommand<TValue>
    where TValue : ICommandValue
{
    public CompositeCommand() : base(default(TValue) ?? throw new InvalidOperationException()) { }

    public Easing Easing => throw new InvalidOperationException();
    public bool Active => true;
    public int Cost => throw new InvalidOperationException();

    public int CompareTo(ICommand? other) => CommandComparer.CompareCommands(this, other);

    public void WriteOsb(TextWriter writer, ExportSettings exportSettings, StoryboardTransform transform, int indentation)
    {
        throw new InvalidOperationException();
    }

    public override string ToString() => $"composite ({StartTime}s - {EndTime}s) : {StartValue} to {EndValue}";
}
