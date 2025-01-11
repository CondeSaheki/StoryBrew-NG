﻿using StoryBrew.Storyboarding.CommandValues;

namespace StoryBrew.Storyboarding.Commands;

public class MoveXCommand : Command<CommandDecimal>
{
    public MoveXCommand(OsbEasing easing, double startTime, double endTime, CommandDecimal startValue, CommandDecimal endValue)
        : base("MX", easing, startTime, endTime, startValue, endValue)
    {
    }

    public override CommandDecimal GetTransformedStartValue(StoryboardTransform transform) => transform.ApplyToPositionX(StartValue);
    public override CommandDecimal GetTransformedEndValue(StoryboardTransform transform) => transform.ApplyToPositionX(EndValue);

    public override CommandDecimal ValueAtProgress(double progress) => StartValue + (EndValue - StartValue) * progress;

    public override CommandDecimal Midpoint(Command<CommandDecimal> endCommand, double progress) => StartValue + (endCommand.EndValue - StartValue) * progress;

    public override IFragmentableCommand GetFragment(double startTime, double endTime)
    {
        if (IsFragmentable)
        {
            var startValue = ValueAtTime(startTime);
            var endValue = ValueAtTime(endTime);
            return new MoveXCommand(Easing, startTime, endTime, startValue, endValue);
        }
        return this;
    }
}