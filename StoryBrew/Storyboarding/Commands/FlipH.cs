using System.Globalization;
using System.Text;

namespace StoryBrew.Storyboarding;

public class FlipH : Command<bool>
{
    public FlipH(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
        : base(easing, startTime, endTime, startValue, endValue)
    {
    }

    public override string ToString() => $"FlipH -> {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "P";
        const string value = "H";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Easing).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{value}";

        writer.AppendLine(result);
    }
}
