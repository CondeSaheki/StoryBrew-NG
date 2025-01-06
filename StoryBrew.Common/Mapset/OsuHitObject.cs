using System.Globalization;
using OpenTK.Mathematics;

namespace StoryBrew.Common.Mapset;

[Serializable]
public class OsuHitObject
{
    public static readonly Vector2 PLAYFIELD_SIZE = new(512, 384);
    public static readonly Vector2 STORYBOARD_SIZE = new(640, 480);
    public static readonly int STORYBOARD_AREA = (int)(STORYBOARD_SIZE.X * STORYBOARD_SIZE.Y);
    public static readonly Vector2 PLAYFIELD_TO_STORYBOARD_OFFSET = new((STORYBOARD_SIZE.X - PLAYFIELD_SIZE.X) * 0.5f, (STORYBOARD_SIZE.Y - PLAYFIELD_SIZE.Y) * 0.75f - 16);
    public static readonly Vector2 WIDESCREEN_STORYBOARD_SIZE = new(STORYBOARD_SIZE.Y * 16f / 9, STORYBOARD_SIZE.Y);
    public static readonly int WIDESCREEN_STORYBOARD_AREA = (int)(WIDESCREEN_STORYBOARD_SIZE.X * WIDESCREEN_STORYBOARD_SIZE.Y);

    public static readonly Box2 STORYBOARD_BOUNDS = new(Vector2.Zero, STORYBOARD_SIZE);
    public static readonly Box2 WIDESCREEN_STORYBOARD_BOUNDS = new((STORYBOARD_SIZE.X - WIDESCREEN_STORYBOARD_SIZE.X) / 2, 0, STORYBOARD_SIZE.X + (WIDESCREEN_STORYBOARD_SIZE.X - STORYBOARD_SIZE.X) / 2, 480);

    public Vector2 PlayfieldPosition;
    public Vector2 Position => PlayfieldPosition + PLAYFIELD_TO_STORYBOARD_OFFSET;

    public virtual Vector2 PlayfieldEndPosition => PlayfieldPositionAtTime(EndTime);
    public Vector2 EndPosition => PlayfieldEndPosition + PLAYFIELD_TO_STORYBOARD_OFFSET;

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
    public Vector2 PositionAtTime(double time) => PlayfieldPositionAtTime(time) + PLAYFIELD_TO_STORYBOARD_OFFSET;

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
