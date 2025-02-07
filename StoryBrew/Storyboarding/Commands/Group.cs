using System.Text;

namespace StoryBrew.Storyboarding;

public abstract class Group : Writable
{
    public IReadOnlyList<Position> Positions => positions;
    public IReadOnlyList<Scale> Scales => scales;
    public IReadOnlyList<ScaleVector> VectorScales => vectorScales;
    public IReadOnlyList<Angle> Angles => angles;
    public IReadOnlyList<Colour> Colours => colours;
    public IReadOnlyList<Alpha> Alphas => alphas;
    public IReadOnlyList<Blending> Blendings => blendings;
    public IReadOnlyList<FlipH> FlipHs => flipHs;
    public IReadOnlyList<FlipV> FlipVs => flipVs;
    public IReadOnlyList<Loop>? Loops => AllowCompound ? loops : null;
    public IReadOnlyList<Trigger>? Triggers => AllowCompound ? triggers : null;

    public IEnumerable<Position> PositionXs => positions.Where(command => command is not PositionY);
    public IEnumerable<Position> PositionYs => positions.Where(command => command is not PositionX);

    internal bool AllowCompound { get; set; }

    public virtual bool HasCommands => commands.Any(lists => lists.Count != 0);
    public virtual double StartTime => commands.SelectMany(list => list).Min(command => command.StartTime);
    public virtual double EndTime => commands.SelectMany(list => list).Max(command => command.EndTime);
    public virtual double Duration => EndTime - StartTime;
    public virtual IEnumerable<ICommand> Commands => commands.SelectMany(list => list);

    private readonly IReadOnlyList<ICommand>[] commands;
    private readonly List<Position> positions = [];
    private readonly List<Scale> scales = [];
    private readonly List<ScaleVector> vectorScales = [];
    private readonly List<Angle> angles = [];
    private readonly List<Colour> colours = [];
    private readonly List<Alpha> alphas = [];
    private readonly List<Blending> blendings = [];
    private readonly List<FlipH> flipHs = [];
    private readonly List<FlipV> flipVs = [];
    private readonly List<Loop> loops = [];
    private readonly List<Trigger> triggers = [];

    public Group(bool allowCompound = true)
    {
        AllowCompound = allowCompound;
        if (allowCompound) commands = [triggers, loops, positions, scales, vectorScales, angles, colours, alphas, blendings, flipHs, flipVs];
        else commands = [positions, scales, vectorScales, angles, colours, alphas, blendings, flipHs, flipVs];
    }

    public void Transform<T>(T command) where T : ICommand
    {
        switch (command)
        {
            case PositionX moveX: positions.Add(moveX); break;
            case PositionY moveY: positions.Add(moveY); break;
            case Position move: positions.Add(move); break;

            case Scale scale: scales.Add(scale); break;
            case ScaleVector vectorScale: vectorScales.Add(vectorScale); break;
            case Angle angle: angles.Add(angle); break;
            case Colour colour: colours.Add(colour); break;
            case Alpha alpha: alphas.Add(alpha); break;
            case Blending blending: blendings.Add(blending); break;
            case FlipH flipH: flipHs.Add(flipH); break;
            case FlipV flipV: flipVs.Add(flipV); break;

            case Trigger trigger:
                if (!AllowCompound) throw new InvalidOperationException();
                triggers.Add(trigger);
                break;
            case Loop loop:
                if (!AllowCompound) throw new InvalidOperationException();
                loops.Add(loop);
                break;

            default: throw new NotImplementedException($"Unhandled command type: {command.GetType()}");
        }
    }

    public void Transform(IEnumerable<ICommand> enumerable)
    {
        foreach (var command in enumerable) Transform(command);
    }

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        if (!HasCommands) throw new InvalidOperationException("Can not write empty group");

        foreach (var command in Commands)
        {
            if (command is Loop || command is Trigger && !AllowCompound) throw new InvalidOperationException("Cannot write compound commands");
            if (command is not Writable writable) throw new InvalidOperationException($"Unhandled command type: {command.GetType()}");

            writable.Write(log, writer, layer, depth);
        }
    }
}
