using StoryBrew.Storyboarding;

namespace StoryBrew.Storyboarding3d;

public interface HasOsbSprites
{
    IEnumerable<OsbSprite> Sprites { get; }
}
