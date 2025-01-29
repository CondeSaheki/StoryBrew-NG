using StoryBrew.Storyboarding;

namespace StoryBrew.Animations;

public static class EasingFunctions
{
    public static double Step(double value) => value >= 1 ? 1 : 0; // Note: not present in Easing enum

    public static double Linear(double value) => value;

    public static double In(double value) => QuadIn(value);   // Note: Check if this is correct
    public static double Out(double value) => QuadOut(value); // Note: Check if this is correct

    public static double QuadIn(double value) => Math.Pow(value, 2);
    public static double QuadOut(double value) => reverse(QuadIn, value);
    public static double QuadInOut(double value) => toInOut(QuadIn, value);

    public static double CubicIn(double value) => Math.Pow(value, 3);
    public static double CubicOut(double value) => reverse(CubicIn, value);
    public static double CubicInOut(double value) => toInOut(CubicIn, value);

    public static double QuartIn(double value) => Math.Pow(value, 4);
    public static double QuartOut(double value) => reverse(QuartIn, value);
    public static double QuartInOut(double value) => toInOut(QuartIn, value);

    public static double QuintIn(double value) => Math.Pow(value, 5);
    public static double QuintOut(double value) => reverse(QuintIn, value);
    public static double QuintInOut(double value) => toInOut(QuintIn, value);

    public static double SineIn(double value) => 1 - Math.Cos(value * Math.PI / 2);
    public static double SineOut(double value) => reverse(SineIn, value);
    public static double SineInOut(double value) => toInOut(SineIn, value);

    public static double ExpoIn(double value) => Math.Pow(2, 10 * (value - 1));
    public static double ExpoOut(double value) => reverse(ExpoIn, value);
    public static double ExpoInOut(double value) => toInOut(ExpoIn, value);

    public static double CircIn(double value) => 1 - Math.Sqrt(1 - Math.Pow(value, 2));
    public static double CircOut(double value) => reverse(CircIn, value);
    public static double CircInOut(double value) => toInOut(CircIn, value);

    public static double BackIn(double value) => Math.Pow(value, 2) * ((1.70158 + 1) * value - 1.70158);
    public static double BackOut(double value) => reverse(BackIn, value);
    public static double BackInOut(double value) => toInOut((y) => Math.Pow(y, 2) * ((1.70158 * 1.525 + 1) * y - 1.70158 * 1.525), value);

    public static double BounceIn(double value) => reverse(BounceOut, value);
    public static double BounceOut(double value) => value < 1 / 2.75 ? 7.5625 * Math.Pow(value, 2) : value < 2 / 2.75 ? 7.5625 * (value -= (1.5 / 2.75)) * value + 0.75 : value < 2.5 / 2.75 ? 7.5625 * (value -= (2.25 / 2.75)) * value + 0.9375 : 7.5625 * (value -= (2.625 / 2.75)) * value + 0.984375;
    public static double BounceInOut(double value) => toInOut(BounceIn, value);

    public static double ElasticIn(double value) => reverse(ElasticOut, value);
    public static double ElasticOut(double value) => Math.Pow(2, -10 * value) * Math.Sin((value - 0.075) * (2 * Math.PI) / 0.3) + 1;
    public static double ElasticOutHalf(double value) => Math.Pow(2, -10 * value) * Math.Sin((0.5 * value - 0.075) * (2 * Math.PI) / 0.3) + 1;
    public static double ElasticOutQuarter(double value) => Math.Pow(2, -10 * value) * Math.Sin((0.25 * value - 0.075) * (2 * Math.PI) / 0.3) + 1;
    public static double ElasticInOut(double value) => toInOut(ElasticIn, value);

    private static double reverse(Func<double, double> function, double value) => 1 - function(1 - value);
    private static double toInOut(Func<double, double> function, double value) => 0.5 * (value < 0.5 ? function(2 * value) : (2 - function(2 - 2 * value)));

    public static double Ease(Easing easing, double value)
    {
        Func<double, double> function = easing switch
        {
            Easing.None => Linear,

            Easing.In => In,
            Easing.Out => Out,

            Easing.InQuad => QuadIn,
            Easing.OutQuad => QuadOut,
            Easing.InOutQuad => QuadInOut,

            Easing.InCubic => CubicIn,
            Easing.OutCubic => CubicOut,
            Easing.InOutCubic => CubicInOut,

            Easing.InQuart => QuartIn,
            Easing.OutQuart => QuartOut,
            Easing.InOutQuart => QuartInOut,

            Easing.InQuint => QuintIn,
            Easing.OutQuint => QuintOut,
            Easing.InOutQuint => QuintInOut,

            Easing.InSine => SineIn,
            Easing.OutSine => SineOut,
            Easing.InOutSine => SineInOut,

            Easing.InExpo => ExpoIn,
            Easing.OutExpo => ExpoOut,
            Easing.InOutExpo => ExpoInOut,

            Easing.InCirc => CircIn,
            Easing.OutCirc => CircOut,
            Easing.InOutCirc => CircInOut,

            Easing.InBack => BackIn,
            Easing.OutBack => BackOut,
            Easing.InOutBack => BackInOut,

            Easing.InBounce => BounceIn,
            Easing.OutBounce => BounceOut,
            Easing.InOutBounce => BounceInOut,

            Easing.InElastic => ElasticIn,
            Easing.OutElastic => ElasticOut,
            Easing.OutElasticHalf => ElasticOutHalf,
            Easing.OutElasticQuarter => ElasticOutQuarter,
            Easing.InOutElastic => ElasticInOut,

            _ => throw new NotImplementedException("Easing function not implemented."),
        };

        return function(value);
    }
}
