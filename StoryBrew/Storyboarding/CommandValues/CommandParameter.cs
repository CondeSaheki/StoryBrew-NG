using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.CommandValues;

[Serializable]
public struct CommandParameter : ICommandValue
{
    public static readonly CommandParameter NONE = new(ParameterType.None);
    public static readonly CommandParameter FLIP_HORIZONTAL = new(ParameterType.FlipHorizontal);
    public static readonly CommandParameter FLIP_VERTICAL = new(ParameterType.FlipVertical);
    public static readonly CommandParameter ADDITIVE_BLENDING = new(ParameterType.AdditiveBlending);

    public readonly ParameterType Type;

    private CommandParameter(ParameterType type)
    {
        Type = type;
    }

    public string ToOsbString(ExportSettings exportSettings)
    {
        switch (Type)
        {
            case ParameterType.FlipHorizontal: return "H";
            case ParameterType.FlipVertical: return "V";
            case ParameterType.AdditiveBlending: return "A";
            default: throw new InvalidOperationException(Type.ToString());
        }
    }

    public override string ToString() => ToOsbString(new());

    public float DistanceFrom(object obj)
    {
        var other = (CommandParameter)obj;
        return other.Type != Type ? 1 : 0;
    }

    public static bool operator ==(CommandParameter left, CommandParameter right) => left.Type == right.Type;

    public static bool operator !=(CommandParameter left, CommandParameter right) => left.Type != right.Type;

    public static implicit operator bool(CommandParameter obj) => obj.Type != ParameterType.None;

    public override bool Equals(object? obj)
    {
        if (obj is CommandParameter other)
        {
            return Type == other.Type;
        }
        return false;
    }

    public override int GetHashCode() => Type.GetHashCode();
}
