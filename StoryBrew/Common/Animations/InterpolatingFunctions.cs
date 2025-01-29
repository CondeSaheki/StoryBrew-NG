using OpenTK.Mathematics;

namespace StoryBrew.Animations;

public static class InterpolatingFunctions
{
    public static T Interpolate<T>(T start, T end, double time)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (end == null) throw new ArgumentNullException(nameof(end));

        dynamic delta = (dynamic)end - (dynamic)start;
        return start + delta * time;
    }

    public static T InterpolateAngle<T>(T start, T end, double time)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (end == null) throw new ArgumentNullException(nameof(end));

        dynamic delta = (dynamic)end - (dynamic)start;

        var difference = delta % MathHelper.TwoPi;
        delta = (2 * difference % MathHelper.TwoPi) - difference;

        return start + delta * time;
    }

    public static Quaternion Interpolate(Quaternion start, Quaternion end, double time) => Quaternion.Slerp(start, end, (float)time);

    public static Color4 Interpolate(Color4 start, Color4 end, double time) => new(
        Interpolate(start.R, end.R, time),
        Interpolate(start.G, end.G, time),
        Interpolate(start.B, end.B, time),
        Interpolate(start.A, end.A, time)
    );
}
