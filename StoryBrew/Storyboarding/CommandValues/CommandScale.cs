using OpenTK.Mathematics;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.CommandValues;

[Serializable]
public struct CommandScale : ICommandValue, IEquatable<CommandScale>
{
    public readonly CommandDecimal X;
    public readonly CommandDecimal Y;

    public CommandScale(CommandDecimal x, CommandDecimal y)
    {
        X = x;
        Y = y;
    }

    public CommandScale(CommandDecimal value) : this(value, value) { }

    public CommandScale(Vector2 vector) : this(vector.X, vector.Y) { }

    public bool Equals(CommandScale other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        return obj is CommandScale commandScale && Equals(commandScale);
    }

    public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();

    public string ToOsbString(ExportSettings exportSettings) => $"{X.ToOsbString(exportSettings)},{Y.ToOsbString(exportSettings)}";

    public override string ToString() => ToOsbString(new());

    public float DistanceFrom(object obj)
    {
        var other = (CommandScale)obj;
        var diffX = X.DistanceFrom(other.X);
        var diffY = Y.DistanceFrom(other.Y);
        return (float)Math.Sqrt((diffX * diffX) + (diffY * diffY));
    }

    public static CommandScale operator +(CommandScale left, CommandScale right) => new(left.X + right.X, left.Y + right.Y);

    public static CommandScale operator -(CommandScale left, CommandScale right) => new(left.X - right.X, left.Y - right.Y);

    public static CommandScale operator *(CommandScale left, CommandScale right) => new(left.X * right.X, left.Y * right.Y);

    public static CommandScale operator *(CommandScale left, double right) => new(left.X * right, left.Y * right);

    public static CommandScale operator *(double left, CommandScale right) => right * left;

    public static CommandScale operator /(CommandScale left, double right) => new(left.X / right, left.Y / right);

    public static bool operator ==(CommandScale left, CommandScale right) => left.Equals(right);

    public static bool operator !=(CommandScale left, CommandScale right) => !left.Equals(right);

    public static implicit operator CommandScale(Vector2 vector) => new(vector);

    public static implicit operator Vector2(CommandScale obj) => new(obj.X, obj.Y);
}
