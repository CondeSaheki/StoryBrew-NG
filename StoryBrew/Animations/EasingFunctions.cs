﻿using StoryBrew.Storyboarding;

namespace StoryBrew.Animations;

public static class EasingFunctions
{
    public static double Reverse(Func<double, double> function, double value) => 1 - function(1 - value);
    public static double ToInOut(Func<double, double> function, double value) => .5 * (value < .5 ? function(2 * value) : (2 - function(2 - 2 * value)));

    public static Func<double, double> Step = x => x >= 1 ? 1 : 0;
    public static Func<double, double> Linear = x => x;

    public static Func<double, double> QuadIn = x => x * x;
    public static Func<double, double> QuadOut = x => Reverse(QuadIn, x);
    public static Func<double, double> QuadInOut = x => ToInOut(QuadIn, x);
    public static Func<double, double> CubicIn = x => x * x * x;
    public static Func<double, double> CubicOut = x => Reverse(CubicIn, x);
    public static Func<double, double> CubicInOut = x => ToInOut(CubicIn, x);
    public static Func<double, double> QuartIn = x => x * x * x * x;
    public static Func<double, double> QuartOut = x => Reverse(QuartIn, x);
    public static Func<double, double> QuartInOut = x => ToInOut(QuartIn, x);
    public static Func<double, double> QuintIn = x => x * x * x * x * x;
    public static Func<double, double> QuintOut = x => Reverse(QuintIn, x);
    public static Func<double, double> QuintInOut = x => ToInOut(QuintIn, x);

    public static Func<double, double> SineIn = x => 1 - Math.Cos(x * Math.PI / 2);
    public static Func<double, double> SineOut = x => Reverse(SineIn, x);
    public static Func<double, double> SineInOut = x => ToInOut(SineIn, x);

    public static Func<double, double> ExpoIn = x => Math.Pow(2, 10 * (x - 1));
    public static Func<double, double> ExpoOut = x => Reverse(ExpoIn, x);
    public static Func<double, double> ExpoInOut = x => ToInOut(ExpoIn, x);

    public static Func<double, double> CircIn = x => 1 - Math.Sqrt(1 - x * x);
    public static Func<double, double> CircOut = x => Reverse(CircIn, x);
    public static Func<double, double> CircInOut = x => ToInOut(CircIn, x);

    public static Func<double, double> BackIn = x => x * x * ((1.70158 + 1) * x - 1.70158);
    public static Func<double, double> BackOut = x => Reverse(BackIn, x);
    public static Func<double, double> BackInOut = x => ToInOut((y) => y * y * ((1.70158 * 1.525 + 1) * y - 1.70158 * 1.525), x);

    public static Func<double, double> BounceOut = x => x < 1 / 2.75 ? 7.5625 * x * x : x < 2 / 2.75 ? 7.5625 * (x -= (1.5 / 2.75)) * x + .75 : x < 2.5 / 2.75 ? 7.5625 * (x -= (2.25 / 2.75)) * x + .9375 : 7.5625 * (x -= (2.625 / 2.75)) * x + .984375;
    public static Func<double, double> BounceIn = x => Reverse(BounceOut, x);
    public static Func<double, double> BounceInOut = x => ToInOut(BounceIn, x);

    public static Func<double, double> ElasticOut = x => Math.Pow(2, -10 * x) * Math.Sin((x - 0.075) * (2 * Math.PI) / .3) + 1;
    public static Func<double, double> ElasticIn = x => Reverse(ElasticOut, x);
    public static Func<double, double> ElasticOutHalf = x => Math.Pow(2, -10 * x) * Math.Sin((0.5 * x - 0.075) * (2 * Math.PI) / .3) + 1;
    public static Func<double, double> ElasticOutQuarter = x => Math.Pow(2, -10 * x) * Math.Sin((0.25 * x - 0.075) * (2 * Math.PI) / .3) + 1;
    public static Func<double, double> ElasticInOut = x => ToInOut(ElasticIn, x);

    public static double Ease(this Easing easing, double value)
        => easing.ToEasingFunction().Invoke(value);

    public static Func<double, double> ToEasingFunction(this Easing easing)
    {
        switch (easing)
        {
            default:
            case Easing.None: return Linear;

            case Easing.In:
            case Easing.InQuad: return QuadIn;
            case Easing.Out:
            case Easing.OutQuad: return QuadOut;
            case Easing.InOutQuad: return QuadInOut;

            case Easing.InCubic: return CubicIn;
            case Easing.OutCubic: return CubicOut;
            case Easing.InOutCubic: return CubicInOut;
            case Easing.InQuart: return QuartIn;
            case Easing.OutQuart: return QuartOut;
            case Easing.InOutQuart: return QuartInOut;
            case Easing.InQuint: return QuintIn;
            case Easing.OutQuint: return QuintOut;
            case Easing.InOutQuint: return QuintInOut;

            case Easing.InSine: return SineIn;
            case Easing.OutSine: return SineOut;
            case Easing.InOutSine: return SineInOut;
            case Easing.InExpo: return ExpoIn;
            case Easing.OutExpo: return ExpoOut;
            case Easing.InOutExpo: return ExpoInOut;
            case Easing.InCirc: return CircIn;
            case Easing.OutCirc: return CircOut;
            case Easing.InOutCirc: return CircInOut;
            case Easing.InElastic: return ElasticIn;
            case Easing.OutElastic: return ElasticOut;
            case Easing.OutElasticHalf: return ElasticOutHalf;
            case Easing.OutElasticQuarter: return ElasticOutQuarter;
            case Easing.InOutElastic: return ElasticInOut;
            case Easing.InBack: return BackIn;
            case Easing.OutBack: return BackOut;
            case Easing.InOutBack: return BackInOut;
            case Easing.InBounce: return BounceIn;
            case Easing.OutBounce: return BounceOut;
            case Easing.InOutBounce: return BounceInOut;
        }
    }
}
