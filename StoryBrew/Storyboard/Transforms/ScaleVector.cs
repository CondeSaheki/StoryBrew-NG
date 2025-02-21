using System.Globalization;
using System.Text;
using OpenTK.Mathematics;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class ScaleVector : Transform<Vector2>
{
    public ScaleVector(Ease ease, double startTime, double endTime, Vector2 startValue, Vector2 endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    public ScaleVector(Ease ease, double startTime, double endTime, int startXValue, int startYValue, int endXValue, int endYValue)
        : base(ease, startTime, endTime, new(startXValue, startYValue), new(endXValue, endYValue))
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandScale startScale, CommandScale endScale) => addCommand(new VScaleCommand(easing, startTime, endTime, startScale, endScale));
    // (Easing.Type easing, double startTime, double endTime, CommandScale startScale, double endX, double endY) => ScaleVec(easing, startTime, endTime, startScale, new CommandScale(endX, endY));
    // (Easing.Type easing, double startTime, double endTime, double startX, double startY, double endX, double endY) => ScaleVec(easing, startTime, endTime, new CommandScale(startX, startY), new CommandScale(endX, endY));
    // (double startTime, double endTime, CommandScale startScale, CommandScale endScale) => ScaleVec(Easing.Type.None, startTime, endTime, startScale, endScale);
    // (double startTime, double endTime, CommandScale startScale, double endX, double endY) => ScaleVec(Easing.Type.None, startTime, endTime, startScale, endX, endY);
    // (double startTime, double endTime, double startX, double startY, double endX, double endY) => ScaleVec(Easing.Type.None, startTime, endTime, startX, startY, endX, endY);
    // (double time, CommandScale scale) => ScaleVec(Easing.Type.None, time, time, scale, scale);
    // (double time, double x, double y) => ScaleVec(Easing.Type.None, time, time, x, y, x, y);

    public override string ToString() => $"VectorScale -> {StartTime} -> {EndTime}, {StartValue.X} -> {EndValue.X}, {StartValue.Y} -> {EndValue.Y} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "V";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Ease).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);
        var startXValue = StartValue.X.ToString(CultureInfo.InvariantCulture);
        var endXValue = EndValue.X.ToString(CultureInfo.InvariantCulture);
        var startYValue = StartValue.Y.ToString(CultureInfo.InvariantCulture);
        var endYValue = EndValue.Y.ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{startXValue},{startYValue},{endXValue},{endYValue}";

        writer.AppendLine(result);
    }
}
