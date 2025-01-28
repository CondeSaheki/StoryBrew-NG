namespace StoryBrew.Storyboarding;

public class PositionX : Position
{
    public PositionX(Easing easing, double startTime, double endTime, float startValue, float endValue)
        : base(easing, startTime, endTime, startValue, default, endValue, default, Axis.X)
    {
    }

    // (Easing easing, double startTime, double endTime, CommandDecimal startX, CommandDecimal endX) => addCommand(new MoveXCommand(easing, startTime, endTime, startX, endX));
    // (double startTime, double endTime, CommandDecimal startX, CommandDecimal endX) => MoveX(Easing.None, startTime, endTime, startX, endX);
    // (double time, CommandDecimal x) => MoveX(Easing.None, time, time, x, x);

    public override string ToString() => base.ToString();
}
