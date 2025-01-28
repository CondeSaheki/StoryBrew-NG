using System.Globalization;

namespace StoryBrew.Storyboarding;

public class Trigger : Group, ICommand
{
    public string Type { get; }
    public override double StartTime { get; }
    public override double EndTime { get; }

    public Trigger(string triggerType, double startTime, double endTime) : base()
    {
        Type = triggerType;
        StartTime = startTime;
        EndTime = endTime;
    }

    public override string ToString() => $"Trigger -> {StartTime} -> {EndTime}, {Type} {Commands.Count()}";

    internal override void Write(uint depth = 0)
    {
        if (!HasCommands) return;

        const string identifier = "T";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        var result = $"{indentation}{identifier},{Type},{startTime},{endTime}";

        base.Write(depth + 1);
    }
}
