using OpenTK.Mathematics;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.CommandValues;

[Serializable]
public struct CommandPosition : ICommandValue, IEquatable<CommandPosition>
{
    public readonly CommandDecimal X;
    public readonly CommandDecimal Y;

    public CommandPosition(double x, double y)
    {
        X = x;
        Y = y;
    }

    public CommandPosition(Vector2 vector) : this(vector.X, vector.Y) { }

    public bool Equals(CommandPosition other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        return obj is CommandPosition commandPosition && Equals(commandPosition);
    }

    public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();

    public string ToOsbString(ExportSettings exportSettings)
    {
        if (exportSettings.UseFloatForMove)
        {
            return $"{X.ToOsbString(exportSettings)},{Y.ToOsbString(exportSettings)}";
        }
        return $"{(int)Math.Round(X)},{(int)Math.Round(Y)}";
    }

    public override string ToString() => ToOsbString(new());

    public float DistanceFrom(object obj) => Distance(this, (CommandPosition)obj);

    public static float Distance(CommandPosition a, CommandPosition b)
    {
        var diffX = a.X - b.X;
        var diffY = a.Y - b.Y;
        return (float)Math.Sqrt((diffX * diffX) + (diffY * diffY));
    }

    public static CommandPosition operator +(CommandPosition left, CommandPosition right) => new(left.X + right.X, left.Y + right.Y);

    public static CommandPosition operator -(CommandPosition left, CommandPosition right) => new(left.X - right.X, left.Y - right.Y);

    public static CommandPosition operator *(CommandPosition left, CommandPosition right) => new(left.X * right.X, left.Y * right.Y);

    public static CommandPosition operator *(CommandPosition left, double right) => new(left.X * right, left.Y * right);

    public static CommandPosition operator *(double left, CommandPosition right) => right * left;

    public static CommandPosition operator /(CommandPosition left, double right) => new(left.X / right, left.Y / right);

    public static bool operator ==(CommandPosition left, CommandPosition right) => left.Equals(right);

    public static bool operator !=(CommandPosition left, CommandPosition right) => !left.Equals(right);

    public static implicit operator Vector2(CommandPosition position) => new(position.X, position.Y);

    public static implicit operator CommandPosition(Vector2 vector) => new(vector.X, vector.Y);
}
