using System.Globalization;
using System.Text;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class Alpha : Transform<float>
{
    public Alpha(Ease ease, double startTime, double endTime, float startValue, float endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandDecimal startOpacity, CommandDecimal endOpacity) => addCommand(new FadeCommand(easing, startTime, endTime, startOpacity, endOpacity));
    // (double startTime, double endTime, CommandDecimal startOpacity, CommandDecimal endOpacity) => Fade(Easing.Type.None, startTime, endTime, startOpacity, endOpacity);
    // (double time, CommandDecimal opacity) => Fade(Easing.Type.None, time, time, opacity, opacity);

    public override string ToString() => $"Alpha: {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "F";
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
