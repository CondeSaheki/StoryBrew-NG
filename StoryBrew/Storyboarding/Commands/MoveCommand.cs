﻿using StoryBrew.Storyboarding.CommandValues;

namespace StoryBrew.Storyboarding.Commands;

public class MoveCommand : Command<CommandPosition>
{
    public MoveCommand(Easing easing, double startTime, double endTime, CommandPosition startValue, CommandPosition endValue)
        : base("M", easing, startTime, endTime, startValue, endValue)
    {
    }

    public override CommandPosition GetTransformedStartValue(StoryboardTransform transform) => transform.ApplyToPosition(StartValue);
    public override CommandPosition GetTransformedEndValue(StoryboardTransform transform) => transform.ApplyToPosition(EndValue);

    public override CommandPosition ValueAtProgress(double progress) => StartValue + (EndValue - StartValue) * progress;

    public override CommandPosition Midpoint(Command<CommandPosition> endCommand, double progress)
        => new(StartValue.X + (endCommand.EndValue.X - StartValue.X) * progress, StartValue.Y + (endCommand.EndValue.Y - StartValue.Y) * progress);

    public override IFragmentableCommand GetFragment(double startTime, double endTime)
    {
        if (IsFragmentable)
        {
            var startValue = ValueAtTime(startTime);
            var endValue = ValueAtTime(endTime);
            return new MoveCommand(Easing, startTime, endTime, startValue, endValue);
        }
        return this;
    }
}
