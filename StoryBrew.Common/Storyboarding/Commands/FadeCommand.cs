﻿using StoryBrew.Common.Storyboarding.CommandValues;

namespace StoryBrew.Common.Storyboarding.Commands;
public class FadeCommand : Command<CommandDecimal>
{
    public FadeCommand(OsbEasing easing, double startTime, double endTime, CommandDecimal startValue, CommandDecimal endValue)
        : base("F", easing, startTime, endTime, startValue, endValue)
    {
    }

    public override CommandDecimal ValueAtProgress(double progress) => StartValue + (EndValue - StartValue) * progress;

    public override CommandDecimal Midpoint(Command<CommandDecimal> endCommand, double progress) => StartValue + (endCommand.EndValue - StartValue) * progress;

    public override IFragmentableCommand GetFragment(double startTime, double endTime)
    {
        if (IsFragmentable)
        {
            var startValue = ValueAtTime(startTime);
            var endValue = ValueAtTime(endTime);
            return new FadeCommand(Easing, startTime, endTime, startValue, endValue);
        }
        return this;
    }
}