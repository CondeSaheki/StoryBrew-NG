﻿using StoryBrew.Storyboarding.CommandValues;

namespace StoryBrew.Storyboarding.Commands;

public class VScaleCommand : Command<CommandScale>
{
    public VScaleCommand(OsbEasing easing, double startTime, double endTime, CommandScale startValue, CommandScale endValue)
        : base("V", easing, startTime, endTime, startValue, endValue)
    {
    }

    public override CommandScale GetTransformedStartValue(StoryboardTransform transform) => transform.ApplyToScale(StartValue);
    public override CommandScale GetTransformedEndValue(StoryboardTransform transform) => transform.ApplyToScale(EndValue);

    public override CommandScale ValueAtProgress(double progress) => StartValue + (EndValue - StartValue) * progress;

    public override CommandScale Midpoint(Command<CommandScale> endCommand, double progress)
        => new(StartValue.X + (endCommand.EndValue.X - StartValue.X) * progress, StartValue.Y + (endCommand.EndValue.Y - StartValue.Y) * progress);

    public override IFragmentableCommand GetFragment(double startTime, double endTime)
    {
        if (IsFragmentable)
        {
            var startValue = ValueAtTime(startTime);
            var endValue = ValueAtTime(endTime);
            return new VScaleCommand(Easing, startTime, endTime, startValue, endValue);
        }
        return this;
    }
}
