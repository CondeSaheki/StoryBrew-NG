﻿using StoryBrew.Storyboarding.CommandValues;

namespace StoryBrew.Storyboarding.Commands;

public class ColorCommand : Command<CommandColor>
{
    public ColorCommand(Easing easing, double startTime, double endTime, CommandColor startValue, CommandColor endValue)
        : base("C", easing, startTime, endTime, startValue, endValue) { }

    public override CommandColor ValueAtProgress(double progress) => StartValue + (EndValue - StartValue) * progress;

    public override CommandColor Midpoint(Command<CommandColor> endCommand, double progress) => StartValue + (endCommand.EndValue - StartValue) * progress;

    public override IFragmentableCommand GetFragment(double startTime, double endTime)
    {
        if (IsFragmentable)
        {
            var startValue = ValueAtTime(startTime);
            var endValue = ValueAtTime(endTime);
            return new ColorCommand(Easing, startTime, endTime, startValue, endValue);
        }
        return this;
    }
}
