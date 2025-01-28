using System.Globalization;

namespace StoryBrew.Storyboarding;

public class Scale : Command<float>
{
    public Scale(Easing easing, double startTime, double endTime, float startValue, float endValue)
        : base(easing, startTime, endTime, startValue, endValue)
    {
    }

    // (Easing easing, double startTime, double endTime, CommandDecimal startScale, CommandDecimal endScale) => addCommand(new ScaleCommand(easing, startTime, endTime, startScale, endScale));
    // (double startTime, double endTime, CommandDecimal startScale, CommandDecimal endScale) => Scale(Easing.None, startTime, endTime, startScale, endScale);
    // (double time, CommandDecimal scale) => Scale(Easing.None, time, time, scale, scale);

    public override string ToString() => $"Scale: {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";

    internal override void Write(uint depth = 0)
    {
        const string identifier = "S";
        const bool float_time = false;

        var indentation = new string(' ', (int)depth);

        var easing = ((int)Easing).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);
        var startValue = ((float)StartValue).ToString(CultureInfo.InvariantCulture);
        var endValue = ((float)EndValue).ToString(CultureInfo.InvariantCulture);

        string result = $"{indentation}{identifier},{easing},{startTime},{endTime},{startValue},{endValue}";
    }
}
