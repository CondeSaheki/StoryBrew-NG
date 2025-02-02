using System.Globalization;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public class Alpha : Command<float>
{
    public Alpha(Easing easing, double startTime, double endTime, float startValue, float endValue)
        : base(easing, startTime, endTime, startValue, endValue)
    {
    }

    // (Easing easing, double startTime, double endTime, CommandDecimal startOpacity, CommandDecimal endOpacity) => addCommand(new FadeCommand(easing, startTime, endTime, startOpacity, endOpacity));
    // (double startTime, double endTime, CommandDecimal startOpacity, CommandDecimal endOpacity) => Fade(Easing.None, startTime, endTime, startOpacity, endOpacity);
    // (double time, CommandDecimal opacity) => Fade(Easing.None, time, time, opacity, opacity);

    public override string ToString() => $"Alpha: {StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";

    internal override void Write(StreamWriter writer, uint depth = 0)
    {
        const string identifier = "F";
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
