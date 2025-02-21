using System.Globalization;
using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class Blending : Transform<bool>
{
    public Blending(Ease ease, double startTime, double endTime, bool startValue, bool endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    public Blending(Ease ease, double startTime, bool startValue)
        : base(ease, startTime, startTime, startValue, startValue)
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandParameter parameter) => addCommand(new ParameterCommand(easing, startTime, endTime, parameter));
    // (double startTime, double endTime) => Parameter(Easing.Type.None, startTime, endTime, CommandParameter.FLIP_HORIZONTAL);
    // (double time) => FlipH(time, time);
    // (double startTime, double endTime) => Parameter(Easing.Type.None, startTime, endTime, CommandParameter.FLIP_VERTICAL);
    // (double time) => FlipV(time, time);
    // (double startTime, double endTime) => Parameter(Easing.Type.None, startTime, endTime, CommandParameter.ADDITIVE_BLENDING);
    // (double time) => Additive(time, time);

    public override string ToString() => $"Blending -> {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "P";
        const string value = "A";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Ease).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{value}";

        writer.AppendLine(result);
    }
}
