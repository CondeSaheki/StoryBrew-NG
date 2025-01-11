using OpenTK.Mathematics;

namespace StoryBrew.Curves;

public interface ICurve
{
    Vector2 StartPosition { get; }
    Vector2 EndPosition { get; }
    double Length { get; }

    Vector2 PositionAtDistance(double distance);
    Vector2 PositionAtDelta(double delta);
}
