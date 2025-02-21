using StoryBrew.Storyboard.Common;

namespace StoryBrew.Storyboard.Transforms;

public class PositionX : Position
{
    public PositionX(Ease ease, double startTime, double endTime, float startValue, float endValue)
        : base(ease, startTime, endTime, startValue, default, endValue, default, Axis.X)
    {
    }

    // (Easing.Type easing, double startTime, double endTime, CommandDecimal startX, CommandDecimal endX) => addCommand(new MoveXCommand(easing, startTime, endTime, startX, endX));
    // (double startTime, double endTime, CommandDecimal startX, CommandDecimal endX) => MoveX(Easing.Type.None, startTime, endTime, startX, endX);
    // (double time, CommandDecimal x) => MoveX(Easing.Type.None, time, time, x, x);

    public override string ToString() => base.ToString();
}
