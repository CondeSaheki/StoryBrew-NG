using System.Globalization;
using System.Text;
using OpenTK.Mathematics;
using SkiaSharp;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Common.Extensions;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class Color : Transform<Color4>
{
    private enum CommandColor
    {
        Both,
        Color,
        Alpha,
    }
    private CommandColor affects;

    public Color(Ease ease, double startTime, double endTime, Vector4 startValue, Vector4 endValue)
        : base(ease, startTime, endTime, (Color4)startValue, (Color4)endValue)
    {
    }

    public Color(Ease ease, double startTime, double endTime, Vector3 startValue, Vector3 endValue)
        : base(ease, startTime, endTime, new(startValue.X, startValue.Y, startValue.Z, 1), new(endValue.X, endValue.Y, endValue.Z, 1))
    {
    }

    public Color(Ease ease, double startTime, double endTime, Color4 startValue, Color4 endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
        var alpha = startValue.A != 1 || endValue.A != 1;
        var color = startValue.R != 1 || startValue.G != 1 || startValue.B != 1 || endValue.R != 1 || endValue.G != 1 || endValue.B != 1;
        affects = alpha && color ? CommandColor.Both : alpha ? CommandColor.Alpha : CommandColor.Color;
    }

    public Color(Ease ease, double startTime, Color4 startValue)
        : base(ease, startTime, startTime, startValue, startValue)
    {
    }

    public Color(Ease ease, double startTime, double endTime, SKColor startValue, SKColor endValue)
        : base(ease, startTime, endTime, startValue.ToColor4(), endValue.ToColor4())
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandColor startColor, CommandColor endColor) => addCommand(new ColorCommand(easing, startTime, endTime, startColor, endColor));
    // (Easing.Type easing, double startTime, double endTime, CommandColor startColor, double endRed, double endGreen, double endBlue) => Color(easing, startTime, endTime, startColor, new CommandColor(endRed, endGreen, endBlue));
    // (Easing.Type easing, double startTime, double endTime, double startRed, double startGreen, double startBlue, double endRed, double endGreen, double endBlue) => Color(easing, startTime, endTime, new CommandColor(startRed, startGreen, startBlue), new CommandColor(endRed, endGreen, endBlue));
    // (double startTime, double endTime, CommandColor startColor, CommandColor endColor) => Color(Easing.Type.None, startTime, endTime, startColor, endColor);
    // (double startTime, double endTime, CommandColor startColor, double endRed, double endGreen, double endBlue) => Color(Easing.Type.None, startTime, endTime, startColor, endRed, endGreen, endBlue);
    // (double startTime, double endTime, double startRed, double startGreen, double startBlue, double endRed, double endGreen, double endBlue) => Color(Easing.Type.None, startTime, endTime, startRed, startGreen, startBlue, endRed, endGreen, endBlue);
    // (double time, CommandColor color) => Color(Easing.Type.None, time, time, color, color);
    // (double time, double red, double green, double blue) => Color(Easing.Type.None, time, time, red, green, blue, red, green, blue);

    // (Easing.Type easing, double startTime, double endTime, CommandColor startColor, double endHue, double endSaturation, double endBrightness) => Color(easing, startTime, endTime, startColor, CommandColor.FromHsb(endHue, endSaturation, endBrightness));
    // (Easing.Type easing, double startTime, double endTime, double startHue, double startSaturation, double startBrightness, double endHue, double endSaturation, double endBrightness) => Color(easing, startTime, endTime, CommandColor.FromHsb(startHue, startSaturation, startBrightness), CommandColor.FromHsb(endHue, endSaturation, endBrightness));
    // (double startTime, double endTime, CommandColor startColor, double endHue, double endSaturation, double endBrightness) => ColorHsb(Easing.Type.None, startTime, endTime, startColor, endHue, endSaturation, endBrightness);
    // (double startTime, double endTime, double startHue, double startSaturation, double startBrightness, double endHue, double endSaturation, double endBrightness) => ColorHsb(Easing.Type.None, startTime, endTime, startHue, startSaturation, startBrightness, endHue, endSaturation, endBrightness);
    // (double time, double hue, double saturation, double brightness) => ColorHsb(Easing.Type.None, time, time, hue, saturation, brightness, hue, saturation, brightness);

    public override string ToString() => $"Color -> {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Ease}";

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "C";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Ease).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        (byte startR, byte startG, byte startB) = ((byte)(StartValue.R * byte.MaxValue), (byte)(StartValue.G * byte.MaxValue), (byte)(StartValue.B * byte.MaxValue));
        (byte endR, byte endG, byte endB) = ((byte)(EndValue.R * byte.MaxValue), (byte)(EndValue.G * byte.MaxValue), (byte)(EndValue.B * byte.MaxValue));

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{startR},{startG},{startB},{endR},{endG},{endB}";

        writer.AppendLine(result);
    }
}
