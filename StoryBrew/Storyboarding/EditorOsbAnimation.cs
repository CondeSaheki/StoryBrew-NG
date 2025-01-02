using StoryBrew.Common.Storyboarding;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding
{
    public class EditorOsbAnimation : OsbAnimation, DisplayableObject, HasPostProcess
    {
        // public void Draw(DrawContext drawContext, Camera camera, Box2 bounds, float opacity, StoryboardTransform transform, Project project, FrameStats frameStats)
        //     => EditorOsbSprite.Draw(drawContext, camera, bounds, opacity, transform, project, frameStats, this);

        public void PostProcess()
        {
            if (InGroup) 
                EndGroup();
        }
    }
}
