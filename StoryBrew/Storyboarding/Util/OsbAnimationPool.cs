namespace StoryBrew.Storyboarding.Util;

public class OsbAnimationPool : OsbSpritePool
{
    private readonly int frameCount;
    private readonly double frameDelay;
    private readonly LoopType loopType;

    public OsbAnimationPool(StoryboardSegment segment, string path, int frameCount, double frameDelay, LoopType loopType, Origin origin, Action<Sprite, double, double>? finalizeSprite = null)
        : base(segment, path, origin, finalizeSprite)
    {
        this.frameCount = frameCount;
        this.frameDelay = frameDelay;
        this.loopType = loopType;
    }

    public OsbAnimationPool(StoryboardSegment segment, string path, int frameCount, double frameDelay, LoopType loopType, Origin origin, bool additive)
        : this(segment, path, frameCount, frameDelay, loopType, origin, additive ? (sprite, startTime, endTime) => sprite.Additive(startTime, endTime) : null)
    {
    }

    protected override Sprite CreateSprite(StoryboardSegment segment, string path, Origin origin)
        => segment.CreateAnimation(path, frameCount, frameDelay, loopType, origin);
}
