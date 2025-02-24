using System.Globalization;
using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class Angle : Transform<float>
{
    public Angle(Ease ease, double startTime, double endTime, float startValue, float endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandDecimal startRotation, CommandDecimal endRotation) => addCommand(new RotateCommand(easing, startTime, endTime, startRotation, endRotation));
    // (double startTime, double endTime, CommandDecimal startRotation, CommandDecimal endRotation) => Rotate(Easing.Type.None, startTime, endTime, startRotation, endRotation);
    // (double time, CommandDecimal rotation) => Rotate(Easing.Type.None, time, time, rotation, rotation);

    public override string ToString() => $"Angle: {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "R";
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
