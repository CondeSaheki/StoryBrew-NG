using StoryBrew.Animations;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding3d;

public class Node3d : Object3d
{
    public readonly KeyframedValue<float> PositionX = new(InterpolatingFunctions.Float, 0);
    public readonly KeyframedValue<float> PositionY = new(InterpolatingFunctions.Float, 0);
    public readonly KeyframedValue<float> PositionZ = new(InterpolatingFunctions.Float, 0);
    public readonly KeyframedValue<float> ScaleX = new(InterpolatingFunctions.Float, 1);
    public readonly KeyframedValue<float> ScaleY = new(InterpolatingFunctions.Float, 1);
    public readonly KeyframedValue<float> ScaleZ = new(InterpolatingFunctions.Float, 1);
    public readonly KeyframedValue<Quaternion> Rotation = new(InterpolatingFunctions.QuaternionSlerp, Quaternion.Identity);

    public override Matrix4 WorldTransformAt(double time)
    {
        return Matrix4.CreateScale(ScaleX.ValueAt(time), ScaleY.ValueAt(time), ScaleZ.ValueAt(time))
            * Matrix4.CreateFromQuaternion(Rotation.ValueAt(time))
            * Matrix4.CreateTranslation(PositionX.ValueAt(time), PositionY.ValueAt(time), PositionZ.ValueAt(time));
    }
}
