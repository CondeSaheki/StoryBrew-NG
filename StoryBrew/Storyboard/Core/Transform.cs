using StoryBrew.Storyboard.Common;

namespace StoryBrew.Storyboard.Core;

public abstract class Transform<T> : Writable, ITransform, IComparable<Transform<T>>
{
    public double StartTime { get; }
    public double EndTime { get; }

    public T StartValue { get; }
    public T EndValue { get; }
    public Ease Ease { get; }

    public double Duration => EndTime - StartTime;

    protected Transform(Ease ease, double startTime, double endTime, T startValue, T endValue)
    {
        if (endTime < startTime) endTime = startTime;

        StartTime = startTime;
        StartValue = startValue;
        EndTime = endTime;
        EndValue = endValue;
        Ease = ease;
    }

    public int CompareTo(Transform<T>? other)
    {
        if (other == null) return 1;

        int result = StartTime.CompareTo(other.StartTime);
        if (result != 0) return result;

        return EndTime.CompareTo(other.EndTime);
    }
}
