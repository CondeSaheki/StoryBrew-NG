namespace StoryBrew.Common.Storyboarding.CommandValues;

[Serializable]
public struct CommandParameter : CommandValue
{
    public static readonly CommandParameter None = new(ParameterType.None);
    public static readonly CommandParameter FlipHorizontal = new(ParameterType.FlipHorizontal);
    public static readonly CommandParameter FlipVertical = new(ParameterType.FlipVertical);
    public static readonly CommandParameter AdditiveBlending = new(ParameterType.AdditiveBlending);

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

    public override string ToString() => ToOsbString(ExportSettings.Default);

    public float DistanceFrom(object obj)
    {
        var other = (CommandParameter)obj;
        return other.Type != Type ? 1 : 0;
    }

    public static bool operator ==(CommandParameter left, CommandParameter right)
        => left.Type == right.Type;

    public static bool operator !=(CommandParameter left, CommandParameter right)
        => left.Type != right.Type;

    public static implicit operator bool(CommandParameter obj)
        => obj.Type != ParameterType.None;

    public override bool Equals(object? obj)
    {
        if (obj is CommandParameter other)
        {
            return Type == other.Type;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }
}
