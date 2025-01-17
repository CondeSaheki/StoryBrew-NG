﻿namespace StoryBrew.Storyboarding.Util;

public class OsbSpritePool : IDisposable
{
    private readonly StoryboardSegment segment;
    private readonly string path;
    private readonly Origin origin;
    private readonly Action<Sprite, double, double>? finalizeSprite;

    private readonly List<PooledSprite> pooledSprites = [];

    public int MaxPoolDuration = 60000;

    public OsbSpritePool(StoryboardSegment segment, string path, Origin origin, Action<Sprite, double, double>? finalizeSprite = null)
    {
        this.segment = segment;
        this.path = path;
        this.origin = origin;
        this.finalizeSprite = finalizeSprite;
    }

    public OsbSpritePool(StoryboardSegment segment, string path, Origin origin, bool additive)
        : this(segment, path, origin, additive ? (sprite, startTime, endTime) => sprite.Additive(startTime, endTime) : null) { }

    public Sprite Get(double startTime, double endTime)
    {
        PooledSprite? result = null;
        foreach (var pooledSprite in pooledSprites)
            if (pooledSprite.EndTime < startTime
                && startTime < pooledSprite.StartTime + MaxPoolDuration
                && (result == null || pooledSprite.StartTime < result.StartTime))
            {
                result = pooledSprite;
            }

        if (result != null)
        {
            result.EndTime = endTime;
            return result.Sprite;
        }

        var sprite = CreateSprite(segment, path, origin);
        pooledSprites.Add(new PooledSprite(sprite, startTime, endTime));
        return sprite;
    }

    public void Clear()
    {
        if (finalizeSprite != null)
            foreach (var pooledSprite in pooledSprites)
            {
                var sprite = pooledSprite.Sprite;
                finalizeSprite(sprite, sprite.CommandsStartTime, pooledSprite.EndTime);
            }

        pooledSprites.Clear();
    }

    protected virtual Sprite CreateSprite(StoryboardSegment segment, string path, Origin origin) => segment.CreateSprite(path, origin);

    private class PooledSprite
    {
        public Sprite Sprite;
        public double StartTime;
        public double EndTime;

        public PooledSprite(Sprite sprite, double startTime, double endTime)
        {
            Sprite = sprite;
            StartTime = startTime;
            EndTime = endTime;
        }
    }

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
