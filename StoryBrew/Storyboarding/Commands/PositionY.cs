namespace StoryBrew.Storyboarding;

public class PositionY : Position
{
    public PositionY(Easing easing, double startTime, double endTime, float startValue, float endValue)
        : base(easing, startTime, endTime, default, startValue, default, endValue, Axis.Y)
    {
    }

    // (Easing easing, double startTime, double endTime, CommandDecimal startY, CommandDecimal endY) => addCommand(new MoveYCommand(easing, startTime, endTime, startY, endY));
    // (double startTime, double endTime, CommandDecimal startY, CommandDecimal endY) => MoveY(Easing.None, startTime, endTime, startY, endY);
    // (double time, CommandDecimal y) => MoveY(Easing.None, time, time, y, y);

    public override string ToString() => base.ToString();
}
