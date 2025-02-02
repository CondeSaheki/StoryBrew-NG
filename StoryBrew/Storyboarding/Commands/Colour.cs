using System.Globalization;
using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Storyboarding;

public class Colour : Command<Color4>
{

    private enum CommandColor
    {
        Both,
        Color,
        Alpha,
    }
    private CommandColor affects;

    public Colour(Easing easing, double startTime, double endTime, Vector4 startValue, Vector4 endValue)
        : base(easing, startTime, endTime, (Color4)startValue, (Color4)endValue)
    {
    }

    public Colour(Easing easing, double startTime, double endTime, Vector3 startValue, Vector3 endValue)
        : base(easing, startTime, endTime, new(startValue.X, startValue.Y, startValue.Z, 1), new(endValue.X, endValue.Y, endValue.Z, 1))
    {
    }

    public Colour(Easing easing, double startTime, double endTime, Color4 startValue, Color4 endValue)
        : base(easing, startTime, endTime, startValue, endValue)
    {
        var alpha = startValue.A != 1 || endValue.A != 1;
        var color = startValue.R != 1 || startValue.G != 1 || startValue.B != 1 || endValue.R != 1 || endValue.G != 1 || endValue.B != 1;
        affects = alpha && color ? CommandColor.Both : alpha ? CommandColor.Alpha : CommandColor.Color;
    }

    public Colour(Easing easing, double startTime, Color4 startValue)
        : base(easing, startTime, startTime, startValue, startValue)
    {
    }

    public Colour(Easing easing, double startTime, double endTime, SKColor startValue, SKColor endValue)
        : base(easing, startTime, endTime, startValue.ToColor4(), endValue.ToColor4())
    {
    }

    // (Easing easing, double startTime, double endTime, CommandColor startColor, CommandColor endColor) => addCommand(new ColorCommand(easing, startTime, endTime, startColor, endColor));
    // (Easing easing, double startTime, double endTime, CommandColor startColor, double endRed, double endGreen, double endBlue) => Color(easing, startTime, endTime, startColor, new CommandColor(endRed, endGreen, endBlue));
    // (Easing easing, double startTime, double endTime, double startRed, double startGreen, double startBlue, double endRed, double endGreen, double endBlue) => Color(easing, startTime, endTime, new CommandColor(startRed, startGreen, startBlue), new CommandColor(endRed, endGreen, endBlue));
    // (double startTime, double endTime, CommandColor startColor, CommandColor endColor) => Color(Easing.None, startTime, endTime, startColor, endColor);
    // (double startTime, double endTime, CommandColor startColor, double endRed, double endGreen, double endBlue) => Color(Easing.None, startTime, endTime, startColor, endRed, endGreen, endBlue);
    // (double startTime, double endTime, double startRed, double startGreen, double startBlue, double endRed, double endGreen, double endBlue) => Color(Easing.None, startTime, endTime, startRed, startGreen, startBlue, endRed, endGreen, endBlue);
    // (double time, CommandColor color) => Color(Easing.None, time, time, color, color);
    // (double time, double red, double green, double blue) => Color(Easing.None, time, time, red, green, blue, red, green, blue);

    // (Easing easing, double startTime, double endTime, CommandColor startColor, double endHue, double endSaturation, double endBrightness) => Color(easing, startTime, endTime, startColor, CommandColor.FromHsb(endHue, endSaturation, endBrightness));
    // (Easing easing, double startTime, double endTime, double startHue, double startSaturation, double startBrightness, double endHue, double endSaturation, double endBrightness) => Color(easing, startTime, endTime, CommandColor.FromHsb(startHue, startSaturation, startBrightness), CommandColor.FromHsb(endHue, endSaturation, endBrightness));
    // (double startTime, double endTime, CommandColor startColor, double endHue, double endSaturation, double endBrightness) => ColorHsb(Easing.None, startTime, endTime, startColor, endHue, endSaturation, endBrightness);
    // (double startTime, double endTime, double startHue, double startSaturation, double startBrightness, double endHue, double endSaturation, double endBrightness) => ColorHsb(Easing.None, startTime, endTime, startHue, startSaturation, startBrightness, endHue, endSaturation, endBrightness);
    // (double time, double hue, double saturation, double brightness) => ColorHsb(Easing.None, time, time, hue, saturation, brightness, hue, saturation, brightness);

    public override string ToString() => $"Color -> {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";

    internal override void Write(StreamWriter writer, uint depth = 0)
    {
        const string identifier = "C";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Easing).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);

        (byte startR, byte startG, byte startB) = ((byte)(StartValue.R * byte.MaxValue), (byte)(StartValue.G * byte.MaxValue), (byte)(StartValue.B * byte.MaxValue));
        (byte endR, byte endG, byte endB) = ((byte)(EndValue.R * byte.MaxValue), (byte)(EndValue.G * byte.MaxValue), (byte)(EndValue.B * byte.MaxValue));

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{startR},{startG},{startB},{endR},{endG},{endB}";

        writer.WriteLine(result);
    }
}
