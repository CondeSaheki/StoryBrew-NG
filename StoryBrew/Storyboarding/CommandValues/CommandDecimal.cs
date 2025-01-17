using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.CommandValues;

[Serializable]
public struct CommandDecimal : ICommandValue, IEquatable<CommandDecimal>
{
    private readonly double value;

    public CommandDecimal(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) throw new InvalidDataException($"Invalid command decimal {value}");
        this.value = value;
    }

    public bool Equals(CommandDecimal other) => value.Equals(other.value);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is CommandDecimal commandDecimal && Equals(commandDecimal);
    }

    public override int GetHashCode() => value.GetHashCode();

    public override string ToString() => ToOsbString(new());

    public float DistanceFrom(object obj) => (float)Math.Abs(value - ((CommandDecimal)obj).value);

    public string ToOsbString(ExportSettings exportSettings) => ((float)value).ToString(exportSettings.NumberFormat);

    public static CommandDecimal operator -(CommandDecimal left, CommandDecimal right) => new(left.value - right.value);

    public static CommandDecimal operator +(CommandDecimal left, CommandDecimal right) => new(left.value + right.value);

    public static bool operator ==(CommandDecimal left, CommandDecimal right) => left.Equals(right);

    public static bool operator !=(CommandDecimal left, CommandDecimal right) => !left.Equals(right);

    public static implicit operator CommandDecimal(double value) => new(value);

    public static implicit operator double(CommandDecimal obj) => obj.value;

    public static implicit operator float(CommandDecimal obj) => (float)obj.value;
}
