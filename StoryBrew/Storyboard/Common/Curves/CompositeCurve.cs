﻿using OpenTK.Mathematics;

namespace StoryBrew.Storyboard.Common.Curves;

[Serializable]
public class CompositeCurve : ICurve
{
    private readonly List<ICurve> curves;
    public IReadOnlyList<ICurve> Curves => curves;

    public Vector2 StartPosition => curves[0].StartPosition;
    public Vector2 EndPosition => curves[^1].EndPosition;

    public double Length
    {
        get
        {
            var length = 0.0;
            foreach (var curve in curves)
                length += curve.Length;
            return length;
        }
    }

    public CompositeCurve(List<ICurve> curves)
    {
        this.curves = [.. curves];
    }

    public Vector2 PositionAtDistance(double distance)
    {
        foreach (var curve in curves)
        {
            if (distance < curve.Length)
                return curve.PositionAtDistance(distance);

            distance -= curve.Length;
        }
        return curves[^1].EndPosition;
    }

    public Vector2 PositionAtDelta(double delta)
    {
        var length = Length;

        var d = delta;
        for (var curveIndex = 0; curveIndex < curves.Count; ++curveIndex)
        {
            var curve = curves[curveIndex];
            var curveDelta = curve.Length / length;

            if (d < curveDelta)
                return curve.PositionAtDelta(d / curveDelta);

            d -= curveDelta;
        }
        return EndPosition;
    }
}
