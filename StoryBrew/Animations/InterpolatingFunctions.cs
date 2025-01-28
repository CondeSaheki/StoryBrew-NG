using OpenTK.Mathematics;

namespace StoryBrew.Animations;

public static class InterpolatingFunctions
{
    public static Func<float, float, double, float> Float = (from, to, progress) => from + (to - from) * (float)progress;
    public static Func<float, float, double, float> FloatAngle = (from, to, progress) => from + (float)(getShortestAngleDelta(from, to) * progress);
    public static Func<double, double, double, double> Double = (from, to, progress) => from + (to - from) * progress;
    public static Func<double, double, double, double> DoubleAngle = (from, to, progress) => from + getShortestAngleDelta(from, to) * progress;
    public static Func<Vector2, Vector2, double, Vector2> Vector2 = (from, to, progress) => from + (to - from) * (float)progress;
    public static Func<Vector3, Vector3, double, Vector3> Vector3 = (from, to, progress) => from + (to - from) * (float)progress;
    public static Func<Quaternion, Quaternion, double, Quaternion> QuaternionSlerp = (from, to, progress) => Quaternion.Slerp(from, to, (float)progress);

    public static Func<bool, bool, double, bool> BoolFrom = (from, to, progress) => from;
    public static Func<bool, bool, double, bool> BoolTo = (from, to, progress) => to;
    public static Func<bool, bool, double, bool> BoolAny = (from, to, progress) => from || to;
    public static Func<bool, bool, double, bool> BoolBoth = (from, to, progress) => from && to;

    public static Func<Color4, Color4, double, Color4> CommandColor = (from, to, progress) => new Color4(
        Float(from.R, to.R, progress),
        Float(from.G, to.G, progress),
        Float(from.B, to.B, progress),
        Float(from.A, to.A, progress)
    );

    private static double getShortestAngleDelta(double from, double to)
    {
        var difference = (to - from) % MathHelper.TwoPi;
        return (2 * difference % MathHelper.TwoPi) - difference;
    }

    internal static float? Interpolate<T>(T from, T to, double progress)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        return (float?)((dynamic)from + ((dynamic)to - (dynamic)from) * (float)progress);
    }
}
