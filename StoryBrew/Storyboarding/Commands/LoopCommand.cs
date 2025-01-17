﻿using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.Commands;

public class LoopCommand : CommandGroup, IFragmentableCommand
{
    public int LoopCount { get; set; }
    public override double EndTime
    {
        get => StartTime + (CommandsEndTime * LoopCount);
        set => LoopCount = (int)Math.Floor((value - StartTime) / CommandsEndTime);
    }

    public LoopCommand(double startTime, int loopCount)
    {
        StartTime = startTime;
        LoopCount = loopCount;
    }

    public override void EndGroup()
    {
        // Make commands loop from their earliest start time (like osu! does)
        var commandsStartTime = CommandsStartTime;
        if (commandsStartTime > 0)
        {
            StartTime += commandsStartTime;
            foreach (var command in Commands)
            {
                if (command is not IOffsetable offsetable) throw new InvalidOperationException("Command must implement IOffsetable");
                offsetable.Offset(-commandsStartTime);
            }
        }
        base.EndGroup();
    }

    protected override string GetCommandGroupHeader(ExportSettings exportSettings)
        => $"L,{((int)StartTime).ToString(exportSettings.NumberFormat)},{LoopCount.ToString(exportSettings.NumberFormat)}";

    public bool IsFragmentable => LoopCount > 1;

    public IFragmentableCommand GetFragment(double startTime, double endTime)
    {
        if (IsFragmentable && (endTime - startTime) % CommandsDuration == 0
                           && (startTime - StartTime) % CommandsDuration == 0)
        {
            var loopCount = (int)Math.Round((endTime - startTime) / CommandsDuration);
            var loopFragment = new LoopCommand(startTime, loopCount);
            foreach (var c in Commands)
                loopFragment.Add(c);

            return loopFragment;
        }
        return this;
    }

    public IEnumerable<int> GetNonFragmentableTimes()
    {
        var nonFragmentableTimes = new HashSet<int>();
        for (var i = 0; i < LoopCount; i++)
            nonFragmentableTimes.UnionWith(Enumerable.Range((int)StartTime + i * (int)CommandsDuration + 1, (int)CommandsDuration - 1));

        return nonFragmentableTimes;
    }
}
