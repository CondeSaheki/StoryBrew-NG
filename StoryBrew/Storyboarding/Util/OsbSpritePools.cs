namespace StoryBrew.Storyboarding.Util;

public class OsbSpritePools : IDisposable
{
    private readonly StoryboardSegment segment;
    private readonly Dictionary<string, OsbSpritePool> pools = [];
    private readonly Dictionary<string, OsbAnimationPool> animationPools = [];

    private int maxPoolDuration = 60000;
    public int MaxPoolDuration
    {
        get => maxPoolDuration;
        set
        {
            if (maxPoolDuration == value) return;
            maxPoolDuration = value;
            foreach (var pool in pools.Values) pool.MaxPoolDuration = maxPoolDuration;
        }
    }

    public OsbSpritePools(StoryboardSegment segment)
    {
        this.segment = segment;
    }

    public void Clear()
    {
        foreach (var pool in pools)
            pool.Value.Clear();
        foreach (var pool in animationPools)
            pool.Value.Clear();
        pools.Clear();
        animationPools.Clear();
    }

    public Sprite Get(double startTime, double endTime, string path, Origin origin = Origin.Centre, Action<Sprite, double, double>? finalizeSprite = null, int poolGroup = 0)
        => getPool(path, origin, finalizeSprite, poolGroup).Get(startTime, endTime);

    public Sprite Get(double startTime, double endTime, string path, Origin origin, bool additive, int poolGroup = 0)
        => Get(startTime, endTime, path, origin, additive ? (sprite, spriteStartTime, spriteEndTime) => sprite.Additive(spriteStartTime, spriteEndTime) : null, poolGroup);

    public Animation Get(double startTime, double endTime, string path, int frameCount, double frameDelay, LoopType loopType, Origin origin = Origin.Centre, Action<Sprite, double, double>? finalizeSprite = null, int poolGroup = 0)
        => (Animation)getPool(path, frameCount, frameDelay, loopType, origin, finalizeSprite, poolGroup).Get(startTime, endTime);

    public Animation Get(double startTime, double endTime, string path, int frameCount, double frameDelay, LoopType loopType, Origin origin, bool additive, int poolGroup = 0)
        => Get(startTime, endTime, path, frameCount, frameDelay, loopType, origin, additive ? (sprite, spriteStartTime, spriteEndTime) => sprite.Additive(spriteStartTime, spriteEndTime) : null, poolGroup);

    private OsbSpritePool getPool(string path, Origin origin, Action<Sprite, double, double>? finalizeSprite, int poolGroup)
    {
        ArgumentNullException.ThrowIfNull(finalizeSprite);

        var key = getKey(path, origin, finalizeSprite, poolGroup);

        if (!pools.TryGetValue(key, out OsbSpritePool? pool))
            pools.Add(key, pool = new OsbSpritePool(segment, path, origin, finalizeSprite) { MaxPoolDuration = maxPoolDuration, });

        return pool;
    }

    private OsbAnimationPool getPool(string path, int frameCount, double frameDelay, LoopType loopType, Origin origin, Action<Sprite, double, double>? finalizeSprite, int poolGroup)
    {
        ArgumentNullException.ThrowIfNull(finalizeSprite);

        var key = getKey(path, frameCount, frameDelay, loopType, origin, finalizeSprite, poolGroup);

        if (!animationPools.TryGetValue(key, out OsbAnimationPool? pool))
            animationPools.Add(key, pool = new OsbAnimationPool(segment, path, frameCount, frameDelay, loopType, origin, finalizeSprite) { MaxPoolDuration = maxPoolDuration, });

        return pool;
    }

    private string getKey(string path, Origin origin, Action<Sprite, double, double> action, int poolGroup)
        => $"{path}#{origin}#{action?.Target}.{action?.Method.Name}#{poolGroup}";

    private string getKey(string path, int frameCount, double frameDelay, LoopType loopType, Origin origin, Action<Sprite, double, double> action, int poolGroup)
        => $"{path}#{frameCount}#{frameDelay}#{loopType}#{origin}#{action?.Target}.{action?.Method.Name}#{poolGroup}";

    #region IDisposable Support

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Clear();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion
}
