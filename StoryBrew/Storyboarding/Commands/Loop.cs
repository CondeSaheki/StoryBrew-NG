using System.Globalization;
using System.Text;

namespace StoryBrew.Storyboarding;

public class Loop : Group, ICommand
{
    public readonly uint TotalIterations;

    public override double StartTime { get; }
    public override double EndTime => StartTime + (base.EndTime - base.StartTime) * TotalIterations;

    public Loop(double startTime, uint repeatCount = 0) : base()
    {
        StartTime = startTime;
        TotalIterations = repeatCount + 1;
    }

    public override string ToString() => $"Loop -> {StartTime} -> {EndTime}, {TotalIterations} {Commands.Count()}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        if (!HasCommands) return;

        const string identifier = "L";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var iterationsValue = TotalIterations.ToString(CultureInfo.InvariantCulture);


        var result = $"{indentation}{identifier},{startTime},{iterationsValue}";

        writer.AppendLine(result);

        base.Write(log, writer, layer, depth + 1);
    }
}
