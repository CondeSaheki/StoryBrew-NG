// Note: EaseMath and Ease are in the same file for now because it is not clear if they should be separated or if they should be combined, definitivly this needs more attention.

namespace StoryBrew.Storyboard.Common;

public enum Ease
{
    None,
    Out,
    In,
    InQuad,
    OutQuad,
    InOutQuad,
    InCubic,
    OutCubic,
    InOutCubic,
    InQuart,
    OutQuart,
    InOutQuart,
    InQuint,
    OutQuint,
    InOutQuint,
    InSine,
    OutSine,
    InOutSine,
    InExpo,
    OutExpo,
    InOutExpo,
    InCirc,
    OutCirc,
    InOutCirc,
    InElastic,
    OutElastic,
    OutElasticHalf,
    OutElasticQuarter,
    InOutElastic,
    InBack,
    OutBack,
    InOutBack,
    InBounce,
    OutBounce,
    InOutBounce,
}

public static class EaseMath
{
    public delegate double EaseDelegate(double value);

    public static double Step(double value) => value >= 1 ? 1 : 0; // Note: not present in Type enum

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

    public static double Apply(Ease ease, double value)
    {
        // Note: This needs profiling to determine if a dictionay or switch statement is faster 
        // anyway I am more inclined to use a switch statement, it do not require heap allocation.

        EaseDelegate function = ease switch
        {
            Ease.None => Linear,

            Ease.In => In,
            Ease.Out => Out,

            Ease.InQuad => QuadIn,
            Ease.OutQuad => QuadOut,
            Ease.InOutQuad => QuadInOut,

            Ease.InCubic => CubicIn,
            Ease.OutCubic => CubicOut,
            Ease.InOutCubic => CubicInOut,

            Ease.InQuart => QuartIn,
            Ease.OutQuart => QuartOut,
            Ease.InOutQuart => QuartInOut,

            Ease.InQuint => QuintIn,
            Ease.OutQuint => QuintOut,
            Ease.InOutQuint => QuintInOut,

            Ease.InSine => SineIn,
            Ease.OutSine => SineOut,
            Ease.InOutSine => SineInOut,

            Ease.InExpo => ExpoIn,
            Ease.OutExpo => ExpoOut,
            Ease.InOutExpo => ExpoInOut,

            Ease.InCirc => CircIn,
            Ease.OutCirc => CircOut,
            Ease.InOutCirc => CircInOut,

            Ease.InBack => BackIn,
            Ease.OutBack => BackOut,
            Ease.InOutBack => BackInOut,

            Ease.InBounce => BounceIn,
            Ease.OutBounce => BounceOut,
            Ease.InOutBounce => BounceInOut,

            Ease.InElastic => ElasticIn,
            Ease.OutElastic => ElasticOut,
            Ease.OutElasticHalf => ElasticOutHalf,
            Ease.OutElasticQuarter => ElasticOutQuarter,
            Ease.InOutElastic => ElasticInOut,

            _ => throw new NotImplementedException("Type function not implemented."),
        };

        return function(value);
    }
}