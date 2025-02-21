using StoryBrew.Storyboard.Common;

namespace StoryBrew.Storyboard.Transforms;

public class PositionY : Position
{
    public PositionY(Ease ease, double startTime, double endTime, float startValue, float endValue)
        : base(ease, startTime, endTime, default, startValue, default, endValue, Axis.Y)
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandDecimal startY, CommandDecimal endY) => addCommand(new MoveYCommand(easing, startTime, endTime, startY, endY));
    // (double startTime, double endTime, CommandDecimal startY, CommandDecimal endY) => MoveY(Easing.Type.None, startTime, endTime, startY, endY);
    // (double time, CommandDecimal y) => MoveY(Easing.Type.None, time, time, y, y);

    public override string ToString() => base.ToString();
}
