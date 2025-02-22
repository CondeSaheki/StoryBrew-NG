using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Storyboard.Common.Beatmap;

public class Beatmap
{
    public Beatmap()
    {
        Log.Warnning("Beatmap is not supported.");
    }
}

// Note: Old Beatmap.cs code for reference

/*

public abstract class Beatmap
{
    /// <summary>
    /// In milliseconds
    /// </summary>
    public const int CONTROL_POINT_LENIENCY = 5;

    /// <summary>
    /// This beatmap diff name, also called version.
    /// </summary>
    public abstract string Name { get; }
    public abstract long Id { get; }

    public abstract double StackLeniency { get; }

    public abstract double HpDrainRate { get; }
    public abstract double CircleSize { get; }
    public abstract double OverallDifficulty { get; }
    public abstract double ApproachRate { get; }
    public abstract double SliderMultiplier { get; }
    public abstract double SliderTickRate { get; }

    public abstract IEnumerable<OsuHitObject> HitObjects { get; }

    /// <summary>
    /// Timestamps in milliseconds of bookmarks
    /// </summary>
    public abstract IEnumerable<int> Bookmarks { get; }

    /// <summary>
    /// Returns all controls points (red or green lines).
    /// </summary>
    public abstract IEnumerable<ControlPoint> ControlPoints { get; }

    /// <summary>
    /// Returns all timing points (red lines).
    /// </summary>
    public abstract IEnumerable<ControlPoint> TimingPoints { get; }

    public abstract IEnumerable<Color4> ComboColors { get; }

    public abstract string BackgroundPath { get; }

    public abstract IEnumerable<OsuBreak> Breaks { get; }

    /// <summary>
    /// Finds the control point (red or green line) active at a specific time.
    /// </summary>
    public abstract ControlPoint? GetControlPointAt(int time);

    /// <summary>
    /// Finds the timing point (red line) active at a specific time.
    /// </summary>
    public abstract ControlPoint? GetTimingPointAt(int time);

    public abstract string AudioFilename { get; }

    public static double GetDifficultyRange(double difficulty, double min, double mid, double max)
    {
        if (difficulty > 5) return mid + (max - mid) * (difficulty - 5) / 5;
        if (difficulty < 5) return mid - (mid - min) * (5 - difficulty) / 5;
        return mid;
    }
}

*/

// Note: Old BeatmapExtensions.cs code for reference

/*

public static class BeatmapExtensions
{
    /// <summary>
    /// Calls tickAction with timingPoint, time, beatCount, tickCount
    /// </summary>
    public static void ForEachTick(this Beatmap beatmap, int startTime, int endTime, int snapDivisor, Action<ControlPoint, double, int, int> tickAction)
    {
        var leftTimingPoint = beatmap.GetTimingPointAt(startTime);
        var timingPoints = beatmap.TimingPoints.GetEnumerator();

        if (timingPoints.MoveNext())
        {
            var timingPoint = timingPoints.Current;
            while (timingPoint != null)
            {
                var nextTimingPoint = timingPoints.MoveNext() ? timingPoints.Current : null;
                if (timingPoint.Offset < leftTimingPoint?.Offset)
                {
                    timingPoint = nextTimingPoint;
                    continue;
                }
                if (timingPoint != leftTimingPoint && endTime + Beatmap.CONTROL_POINT_LENIENCY < timingPoint.Offset) break;

                int tickCount = 0, beatCount = 0;
                var step = Math.Max(1, timingPoint.BeatDuration / snapDivisor);
                var sectionStartTime = timingPoint.Offset;
                var sectionEndTime = Math.Min(nextTimingPoint?.Offset ?? endTime, endTime);
                if (timingPoint == leftTimingPoint)
                    while (startTime < sectionStartTime)
                    {
                        sectionStartTime -= step;
                        tickCount--;
                        if (tickCount % snapDivisor == 0)
                            beatCount--;
                    }

                for (var time = sectionStartTime; time < sectionEndTime + Beatmap.CONTROL_POINT_LENIENCY; time += step)
                {
                    if (startTime < time)
                        tickAction(timingPoint, time, beatCount, tickCount);

                    if (tickCount % snapDivisor == 0)
                        beatCount++;
                    tickCount++;
                }
                timingPoint = nextTimingPoint;
            }
        }
    }

    public static void AsSliderNodes(this IEnumerable<OsuHitObject> hitobjects, Action<OsuSliderNode, OsuHitObject> action)
    {
        foreach (var hitobject in hitobjects)
        {
            switch (hitobject)
            {
                case OsuCircle circle:
                    action(new OsuSliderNode
                    {
                        Time = circle.StartTime,
                        Additions = circle.Additions,
                        SampleSet = circle.SampleSet,
                        AdditionsSampleSet = circle.AdditionsSampleSet,
                        CustomSampleSet = circle.CustomSampleSet,
                        Volume = circle.Volume,
                    }, hitobject);
                    break;

                case OsuSlider slider:
                    foreach (var node in slider.Nodes)
                        action(node, hitobject);
                    break;

                case OsuSpinner spinner:
                    action(new OsuSliderNode
                    {
                        Time = spinner.EndTime,
                        Additions = spinner.Additions,
                        SampleSet = spinner.SampleSet,
                        AdditionsSampleSet = spinner.AdditionsSampleSet,
                        CustomSampleSet = spinner.CustomSampleSet,
                        Volume = spinner.Volume,
                    }, hitobject);
                    break;
            }
        }
    }
}

*/