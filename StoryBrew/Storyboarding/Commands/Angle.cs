using System.Globalization;

namespace StoryBrew.Storyboarding;

public class Angle : Command<float>
{
    public Angle(Easing easing, double startTime, double endTime, float startValue, float endValue)
        : base(easing, startTime, endTime, startValue, endValue)
    {
    }

    // (Easing easing, double startTime, double endTime, CommandDecimal startRotation, CommandDecimal endRotation) => addCommand(new RotateCommand(easing, startTime, endTime, startRotation, endRotation));
    // (double startTime, double endTime, CommandDecimal startRotation, CommandDecimal endRotation) => Rotate(Easing.None, startTime, endTime, startRotation, endRotation);
    // (double time, CommandDecimal rotation) => Rotate(Easing.None, time, time, rotation, rotation);

    public override string ToString() => $"Angle: {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";

    internal override void Write(StreamWriter writer, uint depth = 0)
    {
        const string identifier = "R";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Easing).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);
        var startValue = ((float)StartValue).ToString(CultureInfo.InvariantCulture);
        var endValue = ((float)EndValue).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{startValue},{endValue}";
        
        writer.WriteLine(result);
    }
}
