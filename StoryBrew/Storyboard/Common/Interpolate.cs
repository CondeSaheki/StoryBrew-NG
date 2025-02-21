using OpenTK.Mathematics;

namespace StoryBrew.Storyboard.Common;

[Obsolete("To be implemented as extension method")]
public static class Interpolate
{
    [Obsolete("To be implemented as extension method")]
    public static T Value<T>(T start, T end, double time)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (end == null) throw new ArgumentNullException(nameof(end));

        dynamic delta = (dynamic)end - (dynamic)start;
        return start + delta * time;
    }

    [Obsolete("To be implemented as extension method")]
    public static T Angle<T>(T start, T end, double time)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (end == null) throw new ArgumentNullException(nameof(end));

        dynamic delta = (dynamic)end - (dynamic)start;

        var difference = delta % MathHelper.TwoPi;
        delta = (2 * difference % MathHelper.TwoPi) - difference;

        return start + delta * time;
    }

    [Obsolete("To be implemented as extension method")]
    public static Quaternion Quaternion(Quaternion start, Quaternion end, double time) => OpenTK.Mathematics.Quaternion.Slerp(start, end, (float)time);

    [Obsolete("To be implemented as extension method")]
    public static Color4 Color(Color4 start, Color4 end, double time) => new(
        Value(start.R, end.R, time),
        Value(start.G, end.G, time),
        Value(start.B, end.B, time),
        Value(start.A, end.A, time)
    );
}
