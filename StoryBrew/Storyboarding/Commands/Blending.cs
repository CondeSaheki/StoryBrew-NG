using System.Globalization;
using System.Text;

namespace StoryBrew.Storyboarding;

public class Blending : Command<bool>
{
    public Blending(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
        : base(easing, startTime, endTime, startValue, endValue)
    {
    }

    public Blending(Easing easing, double startTime, bool startValue)
        : base(easing, startTime, startTime, startValue, startValue)
    {
    }

    // (Easing easing, double startTime, double endTime, CommandParameter parameter) => addCommand(new ParameterCommand(easing, startTime, endTime, parameter));
    // (double startTime, double endTime) => Parameter(Easing.None, startTime, endTime, CommandParameter.FLIP_HORIZONTAL);
    // (double time) => FlipH(time, time);
    // (double startTime, double endTime) => Parameter(Easing.None, startTime, endTime, CommandParameter.FLIP_VERTICAL);
    // (double time) => FlipV(time, time);
    // (double startTime, double endTime) => Parameter(Easing.None, startTime, endTime, CommandParameter.ADDITIVE_BLENDING);
    // (double time) => Additive(time, time);

    public override string ToString() => $"Blending -> {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "P";
        const string value = "A";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Easing).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{value}";

        writer.AppendLine(result);
    }
}
