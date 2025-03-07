using System.Globalization;
using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class FlipVertical : Transform<bool>
{
    public FlipVertical(Ease ease, double startTime, double endTime, bool startValue, bool endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    public FlipVertical(Ease ease, double startTime, bool startValue)
        : base(ease, startTime, startTime, startValue, startValue)
    {
    }

    public override string ToString() => $"FlipV -> {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "P";
        const string value = "V";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Ease).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{value}";

        writer.AppendLine(result);
    }
}
