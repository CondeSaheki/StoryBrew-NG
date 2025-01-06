namespace StoryBrew.Common.Animations;

public struct Keyframe<TValue> : IComparable<Keyframe<TValue>>
{
    public readonly double Time;
    public readonly TValue Value;
    public readonly Func<double, double> Ease;

    public Keyframe(double time)
        : this(time, default(TValue) ?? throw new Exception())
    {
    }

    public Keyframe(double time, TValue value, Func<double, double>? easing = null)
    {
        Time = time;
        Value = value;
        Ease = easing ?? EasingFunctions.Linear;
    }

    public Keyframe<TValue> WithTime(double time) => new(time, Value, Ease);

    public Keyframe<TValue> WithValue(TValue value) => new(Time, value, Ease);

    public int CompareTo(Keyframe<TValue> other) => Math.Sign(Time - other.Time);

    public override string ToString() => $"{Time:0.000}s {typeof(TValue)}:{Value}";
}
