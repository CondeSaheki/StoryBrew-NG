﻿using StoryBrew.Curves;
using System.Globalization;
using OpenTK.Mathematics;

namespace StoryBrew.Mapset;

[Serializable]
public class OsuSlider : OsuHitObject
{
    private readonly List<OsuSliderNode> nodes;
    public IEnumerable<OsuSliderNode> Nodes => nodes;
    public int NodeCount => nodes.Count;

    private readonly List<OsuSliderControlPoint> controlPoints;
    public IEnumerable<OsuSliderControlPoint> ControlPoints => controlPoints;
    public int ControlPointCount => controlPoints.Count;

    public override double EndTime => StartTime + TravelCount * TravelDuration;

    private ICurve? curve;
    public ICurve Curve
    {
        get
        {
            if (curve == null) generateCurve();
            return curve ?? throw new Exception();
        }
    }

    private Vector2 playfieldTipPosition;
    public Vector2 PlayfieldTipPosition
    {
        get
        {
            if (curve == null) generateCurve();
            return playfieldTipPosition;
        }
    }

    public Vector2 TipPosition => PlayfieldTipPosition + PLAYFIELD_TO_STORYBOARD_OFFSET;

    /// <summary>
    /// The total distance the slider ball travels, in osu!pixels.
    /// </summary>
    public double Length;

    /// <summary>
    /// The time it takes for the slider ball to travels across the slider's body in beats.
    /// </summary>
    public double TravelDurationBeats;

    /// <summary>
    /// The time it takes for the slider ball to travels across the slider's body in milliseconds.
    /// </summary>
    public double TravelDuration;

    /// <summary>
    /// How many times the slider ball travels across the slider's body.
    /// </summary>
    public int TravelCount => nodes.Count - 1;

    /// <summary>
    /// How many times the slider ball hits a repeat.
    /// </summary>
    public int RepeatCount => nodes.Count - 2;

    public SliderCurveType CurveType;

    public OsuSlider(List<OsuSliderNode> nodes, List<OsuSliderControlPoint> controlPoints)
    {
        this.nodes = nodes;
        this.controlPoints = controlPoints;
    }

    public override Vector2 PlayfieldPositionAtTime(double time)
    {
        if (time <= StartTime)
            return PlayfieldPosition;

        if (EndTime <= time)
            return TravelCount % 2 == 0 ? PlayfieldPosition : PlayfieldTipPosition;

        var elapsedSinceStartTime = time - StartTime;

        var repeatAtTime = 1;
        var progressDuration = elapsedSinceStartTime;
        while (progressDuration > TravelDuration)
        {
            progressDuration -= TravelDuration;
            ++repeatAtTime;
        }

        var progress = progressDuration / TravelDuration;
        var reversed = repeatAtTime % 2 == 0;
        if (reversed) progress = 1.0 - progress;

        if (curve == null) generateCurve();
        return curve?.PositionAtDistance(Length * progress) ?? throw new Exception();
    }

    public override string ToString()
        => $"{base.ToString()}, {CurveType}, {TravelCount}x";

    private void generateCurve()
    {
        switch (CurveType)
        {
            case SliderCurveType.Catmull:
                if (controlPoints.Count == 1)
                    goto case SliderCurveType.Linear;
                curve = generateCatmullCurve();
                break;
            case SliderCurveType.Bezier:
                if (controlPoints.Count == 1)
                    goto case SliderCurveType.Linear;
                curve = generateBezierCurve();
                break;
            case SliderCurveType.Perfect:
                if (controlPoints.Count > 2)
                    goto case SliderCurveType.Bezier;
                if (controlPoints.Count < 2 ||
                    !CircleCurve.IsValid(PlayfieldPosition, controlPoints[0].PlayfieldPosition, controlPoints[1].PlayfieldPosition))
                    goto case SliderCurveType.Linear;
                curve = generateCircleCurve();
                break;
            case SliderCurveType.Linear:
            default:
                curve = generateLinearCurve();
                break;
        }
        playfieldTipPosition = curve.PositionAtDistance(Length);
    }

    private ICurve generateCircleCurve()
        => new CircleCurve(PlayfieldPosition, controlPoints[0].PlayfieldPosition, controlPoints[1].PlayfieldPosition);

    private ICurve generateBezierCurve()
    {
        var curves = new List<ICurve>();

        var curvePoints = new List<Vector2>();
        var precision = (int)Math.Ceiling(Length);

        var previousPosition = PlayfieldPosition;
        curvePoints.Add(previousPosition);

        foreach (var controlPoint in controlPoints)
        {
            if (controlPoint.PlayfieldPosition == previousPosition)
            {
                if (curvePoints.Count > 1)
                    curves.Add(new Curves.BezierCurve(curvePoints, precision));

                curvePoints = new List<Vector2>();
            }

            curvePoints.Add(controlPoint.PlayfieldPosition);
            previousPosition = controlPoint.PlayfieldPosition;
        }

        if (curvePoints.Count > 1)
            curves.Add(new Curves.BezierCurve(curvePoints, precision));

        return new CompositeCurve(curves);
    }

    private ICurve generateCatmullCurve()
    {
        List<Vector2> curvePoints = new(controlPoints.Count + 1)
        {
            PlayfieldPosition
        };
        foreach (var controlPoint in controlPoints)
            curvePoints.Add(controlPoint.PlayfieldPosition);

        var precision = (int)Math.Ceiling(Length);
        return new CatmullCurve(curvePoints, precision);
    }

    private ICurve generateLinearCurve()
    {
        var curves = new List<ICurve>();

        var previousPoint = PlayfieldPosition;
        foreach (var controlPoint in controlPoints)
        {
            curves.Add(new Curves.BezierCurve(
            [
                previousPoint,
                controlPoint.PlayfieldPosition,
            ], 0));
            previousPoint = controlPoint.PlayfieldPosition;
        }
        return new CompositeCurve(curves);
    }

    public static OsuSlider Parse(Beatmap beatmap, string[] values, int x, int y, double startTime, HitObjectFlag flags, HitSoundAddition additions, ControlPoint timingPoint, ControlPoint controlPoint, SampleSet sampleSet, SampleSet additionsSampleSet, int customSampleSet, float volume)
    {
        var slider = values[5];
        var sliderValues = slider.Split('|');

        var curveType = LetterToCurveType(sliderValues[0]);
        var sliderControlPointCount = sliderValues.Length - 1;
        var sliderControlPoints = new List<OsuSliderControlPoint>(sliderControlPointCount);
        for (var i = 0; i < sliderControlPointCount; i++)
        {
            var controlPointValues = sliderValues[i + 1].Split(':');
            var controlPointX = float.Parse(controlPointValues[0], CultureInfo.InvariantCulture);
            var controlPointY = float.Parse(controlPointValues[1], CultureInfo.InvariantCulture);
            sliderControlPoints.Add(new Vector2(controlPointX, controlPointY));
        }

        var nodeCount = int.Parse(values[6]) + 1;
        var length = double.Parse(values[7], CultureInfo.InvariantCulture);

        var sliderMultiplierLessLength = length / beatmap.SliderMultiplier;
        var travelDurationBeats = sliderMultiplierLessLength / 100 * controlPoint.SliderMultiplier;
        var travelDuration = timingPoint.BeatDuration * travelDurationBeats;

        var sliderNodes = new List<OsuSliderNode>(nodeCount);
        for (var i = 0; i < nodeCount; i++)
        {
            var nodeStartTime = startTime + i * travelDuration;
            var nodeControlPoint = beatmap.GetTimingPointAt((int)nodeStartTime);
            sliderNodes.Add(new OsuSliderNode()
            {
                Time = nodeStartTime,
                SampleSet = nodeControlPoint?.SampleSet ?? throw new Exception(),
                AdditionsSampleSet = nodeControlPoint.SampleSet,
                CustomSampleSet = nodeControlPoint.CustomSampleSet,
                Volume = nodeControlPoint.Volume,
                Additions = additions,
            });
        }
        if (values.Length > 8)
        {
            var sliderAddition = values[8];
            var sliderAdditionValues = sliderAddition.Split('|');
            for (var i = 0; i < sliderAdditionValues.Length; i++)
            {
                var node = sliderNodes[i];
                var nodeAdditions = (HitSoundAddition)int.Parse(sliderAdditionValues[i]);
                node.Additions = nodeAdditions;
            }
        }
        if (values.Length > 9)
        {
            var sampleAndAdditionSampleSet = values[9];
            var sampleAndAdditionSampleSetValues = sampleAndAdditionSampleSet.Split('|');
            for (var i = 0; i < sampleAndAdditionSampleSetValues.Length; i++)
            {
                var node = sliderNodes[i];
                var sampleAndAdditionSampleSetValues2 = sampleAndAdditionSampleSetValues[i].Split(':');
                var nodeSampleSet = (SampleSet)int.Parse(sampleAndAdditionSampleSetValues2[0]);
                var nodeAdditionsSampleSet = (SampleSet)int.Parse(sampleAndAdditionSampleSetValues2[1]);

                if (nodeSampleSet != 0)
                {
                    node.SampleSet = nodeSampleSet;
                    node.AdditionsSampleSet = nodeSampleSet;
                }
                if (nodeAdditionsSampleSet != 0)
                    node.AdditionsSampleSet = nodeAdditionsSampleSet;
            }
        }

        string samplePath = string.Empty;
        if (values.Length > 10)
        {
            var special = values[10];
            var specialValues = special.Split(':');
            var objectSampleSet = (SampleSet)int.Parse(specialValues[0]);
            var objectAdditionsSampleSet = (SampleSet)int.Parse(specialValues[1]);
            var objectCustomSampleSet = 0;
            if (specialValues.Length > 2)
                objectCustomSampleSet = int.Parse(specialValues[2]);
            var objectVolume = 0.0f;
            if (specialValues.Length > 3)
                objectVolume = int.Parse(specialValues[3]);
            if (specialValues.Length > 4)
                samplePath = specialValues[4];

            if (objectSampleSet != 0)
            {
                sampleSet = objectSampleSet;
                additionsSampleSet = objectSampleSet;
            }
            if (objectAdditionsSampleSet != 0)
                additionsSampleSet = objectAdditionsSampleSet;
            if (objectCustomSampleSet != 0)
                customSampleSet = objectCustomSampleSet;
            if (objectVolume > 0.001f)
                volume = objectVolume;
        }

        return new OsuSlider(sliderNodes, sliderControlPoints)
        {
            PlayfieldPosition = new Vector2(x, y),
            StartTime = startTime,
            Flags = flags,
            Additions = additions,
            SampleSet = sampleSet,
            AdditionsSampleSet = additionsSampleSet,
            CustomSampleSet = customSampleSet,
            Volume = volume,
            SamplePath = samplePath,
            // Slider specific
            CurveType = curveType,
            Length = length,
            TravelDurationBeats = travelDurationBeats,
            TravelDuration = travelDuration,
        };
    }

    public static SliderCurveType LetterToCurveType(string letter)
    {
        switch (letter)
        {
            case "L": return SliderCurveType.Linear;
            case "C": return SliderCurveType.Catmull;
            case "B": return SliderCurveType.Bezier;
            case "P": return SliderCurveType.Perfect;
            default: return SliderCurveType.Unknown;
        }
    }
}

[Serializable]
public class OsuSliderNode
{
    public double Time;
    public HitSoundAddition Additions;
    public SampleSet SampleSet;
    public SampleSet AdditionsSampleSet;
    public int CustomSampleSet;
    public float Volume;
}

[Serializable]
public struct OsuSliderControlPoint
{
    public Vector2 PlayfieldPosition;
    public Vector2 Position => PlayfieldPosition + OsuHitObject.PLAYFIELD_TO_STORYBOARD_OFFSET;

    public static implicit operator OsuSliderControlPoint(Vector2 vector2)
        => new OsuSliderControlPoint() { PlayfieldPosition = vector2, };
}

public enum SliderCurveType
{
    Unknown,
    Linear,
    Catmull,
    Bezier,
    Perfect,
}
