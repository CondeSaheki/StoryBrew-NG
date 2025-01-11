using OpenTK.Mathematics;
using StoryBrew.Mapset;

namespace StoryBrew.Storyboarding3d;

public abstract class Camera
{
    public Vector2 Resolution = new(1366, 768);
    public double ResolutionScale => OsuHitObject.STORYBOARD_SIZE.Y / Resolution.Y;
    public double AspectRatio => Resolution.X / Resolution.Y;

    public float DistanceForHorizontalFov(double fov)
    {
        return (float)(Resolution.X * 0.5 / Math.Tan(MathHelper.DegreesToRadians(fov) * 0.5));
    }
    public float DistanceForVerticalFov(double fov)
    {
        return (float)(Resolution.Y * 0.5 / Math.Tan(MathHelper.DegreesToRadians(fov) * 0.5));
    }

    public abstract CameraState StateAt(double time);
}
