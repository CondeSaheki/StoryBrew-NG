using StoryBrew.Common.Storyboarding.CommandValues;
using OpenTK.Mathematics;

namespace StoryBrew.Common.Storyboarding3d;

public class Object3dState
{
    public static readonly Object3dState INITIAL_STATE = new(Matrix4.Identity, CommandColor.WHITE, 1);

    public readonly Matrix4 WorldTransform;
    public readonly CommandColor Color;
    public readonly float Opacity;

    public Object3dState(Matrix4 worldTransform, CommandColor color, float opacity)
    {
        WorldTransform = worldTransform;
        Color = color;
        Opacity = opacity;
    }
}
