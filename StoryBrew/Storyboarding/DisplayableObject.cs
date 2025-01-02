using StoryBrew.Common.Storyboarding;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding
{
    public interface DisplayableObject
    {
        double StartTime { get; }
        double EndTime { get; }

        // void Draw(DrawContext drawContext, Camera camera, Box2 bounds, float opacity, StoryboardTransform transform, Project project, FrameStats frameStats);
    }
}
