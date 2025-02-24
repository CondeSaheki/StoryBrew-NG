using System.Globalization;
using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class Scale : Transform<float>
{
    public Scale(Ease ease, double startTime, double endTime, float startValue, float endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandDecimal startScale, CommandDecimal endScale) => addCommand(new ScaleCommand(easing, startTime, endTime, startScale, endScale));
    // (double startTime, double endTime, CommandDecimal startScale, CommandDecimal endScale) => Scale(Easing.Type.None, startTime, endTime, startScale, endScale);
    // (double time, CommandDecimal scale) => Scale(Easing.Type.None, time, time, scale, scale);

    public override string ToString() => $"Scale: {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "S";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Ease).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);
        var startValue = ((float)StartValue).ToString(CultureInfo.InvariantCulture);
        var endValue = ((float)EndValue).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{startValue},{endValue}";

        writer.AppendLine(result);
    }
}
