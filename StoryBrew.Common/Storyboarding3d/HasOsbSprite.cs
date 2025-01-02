using StoryBrew.Common.Storyboarding;

namespace StoryBrew.Common.Storyboarding3d;

public interface HasOsbSprites
{
    IEnumerable<OsbSprite> Sprites { get; }
}
