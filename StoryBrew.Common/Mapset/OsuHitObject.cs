﻿using System.Globalization;
using OpenTK.Mathematics;

namespace StoryBrew.Common.Mapset;

[Serializable]
public class OsuHitObject
{
    public static readonly Vector2 PlayfieldSize = new Vector2(512, 384);
    public static readonly Vector2 StoryboardSize = new Vector2(640, 480);
    public static readonly int StoryboardArea = (int)(StoryboardSize.X * StoryboardSize.Y);
    public static readonly Vector2 PlayfieldToStoryboardOffset = new Vector2((StoryboardSize.X - PlayfieldSize.X) * 0.5f, (StoryboardSize.Y - PlayfieldSize.Y) * 0.75f - 16);
    public static readonly Vector2 WidescreenStoryboardSize = new Vector2(StoryboardSize.Y * 16f / 9, StoryboardSize.Y);
    public static readonly int WidescreenStoryboardArea = (int)(WidescreenStoryboardSize.X * WidescreenStoryboardSize.Y);

    public static readonly Box2 StoryboardBounds = new Box2(Vector2.Zero, StoryboardSize);
    public static readonly Box2 WidescreenStoryboardBounds = new Box2((StoryboardSize.X - WidescreenStoryboardSize.X) / 2, 0, StoryboardSize.X + (WidescreenStoryboardSize.X - StoryboardSize.X) / 2, 480);

    public Vector2 PlayfieldPosition;
    public Vector2 Position => PlayfieldPosition + PlayfieldToStoryboardOffset;

    public virtual Vector2 PlayfieldEndPosition => PlayfieldPositionAtTime(EndTime);
    public Vector2 EndPosition => PlayfieldEndPosition + PlayfieldToStoryboardOffset;

    public double StartTime;
    public virtual double EndTime => StartTime;

    public HitObjectFlag Flags;
    public HitSoundAddition Additions;
    public SampleSet SampleSet;
    public SampleSet AdditionsSampleSet;
    public int CustomSampleSet;
    public float Volume;
    public string? SamplePath;

    public int ComboIndex = 1;
    public int ColorIndex = 0;
    public Color4 Color = Color4.White;

    public bool NewCombo => (Flags & HitObjectFlag.NewCombo) > 0;
    public int ComboOffset => ((int)Flags >> 4) & 7;

    public int StackIndex;
    public Vector2 StackOffset;

    public virtual Vector2 PlayfieldPositionAtTime(double time) => PlayfieldPosition;
    public Vector2 PositionAtTime(double time) => PlayfieldPositionAtTime(time) + PlayfieldToStoryboardOffset;

    public override string ToString()
        => $"{(int)StartTime}, {Flags}";

    public static OsuHitObject? Parse(Beatmap beatmap, string line)
    {
        var values = line.Split(',');

        var x = int.Parse(values[0]);
        var y = int.Parse(values[1]);
        var startTime = double.Parse(values[2], CultureInfo.InvariantCulture);
        var flags = (HitObjectFlag)int.Parse(values[3]);
        var additions = (HitSoundAddition)int.Parse(values[4]);

        var timingPoint = beatmap.GetTimingPointAt((int)startTime) ?? throw new Exception();
        var controlPoint = beatmap.GetControlPointAt((int)startTime) ?? throw new Exception();

        var sampleSet = controlPoint.SampleSet;
        var additionsSampleSet = controlPoint.SampleSet;
        var customSampleSet = controlPoint.CustomSampleSet;
        var volume = controlPoint.Volume;

        if (flags.HasFlag(HitObjectFlag.Circle))
            return OsuCircle.Parse(beatmap, values, x, y, startTime, flags, additions, timingPoint, controlPoint, sampleSet, additionsSampleSet, customSampleSet, volume);
        else if (flags.HasFlag(HitObjectFlag.Slider))
            return OsuSlider.Parse(beatmap, values, x, y, startTime, flags, additions, timingPoint, controlPoint, sampleSet, additionsSampleSet, customSampleSet, volume);
        else if (flags.HasFlag(HitObjectFlag.Hold))
            return OsuHold.Parse(beatmap, values, x, y, startTime, flags, additions, timingPoint, controlPoint, sampleSet, additionsSampleSet, customSampleSet, volume);
        else if (flags.HasFlag(HitObjectFlag.Spinner))
            return OsuSpinner.Parse(beatmap, values, x, y, startTime, flags, additions, timingPoint, controlPoint, sampleSet, additionsSampleSet, customSampleSet, volume);
        return null;
    }
}

[Flags]
public enum HitObjectFlag
{
    Circle = 1,
    Slider = 2,
    NewCombo = 4,
    Spinner = 8,
    SkipColor1 = 16,
    SkipColor2 = 32,
    SkipColor3 = 64,
    Hold = 128,
    Colors = SkipColor1 | SkipColor2 | SkipColor3,
}

[Flags]
public enum HitSoundAddition
{
    None = 0,
    Normal = 1,
    Whistle = 2,
    Finish = 4,
    Clap = 8,
}

public enum SampleSet
{
    None = 0,
    Normal = 1,
    Soft = 2,
    Drum = 3
}
