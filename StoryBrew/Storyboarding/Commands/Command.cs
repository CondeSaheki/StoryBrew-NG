namespace StoryBrew.Storyboarding;

public abstract class Command<T> : Writable, ICommand, IComparable<Command<T>>
{
    public double StartTime { get; }
    public double EndTime { get; }

    public T StartValue { get; }
    public T EndValue { get; }
    public Easing Easing { get; }

    public double Duration => EndTime - StartTime;

    protected Command(Easing easing, double startTime, double endTime, T startValue, T endValue)
    {
        if (endTime < startTime) endTime = startTime;

        StartTime = startTime;
        StartValue = startValue;
        EndTime = endTime;
        EndValue = endValue;
        Easing = easing;
    }

    public int CompareTo(Command<T>? other)
    {
        if (other == null) return 1;

        int result = StartTime.CompareTo(other.StartTime);
        if (result != 0) return result;

        return EndTime.CompareTo(other.EndTime);
    }
}
