using OpenTK.Mathematics;

namespace StoryBrew.Common.Util;

internal static class Box2Extensions
{
    /// <summary>
    /// Assumes that the Box2 intersect.
    /// </summary>
    public static Box2 IntersectWith(this Box2 box2, Box2 other)
        => new Box2(Math.Max(box2.Min.X, other.Min.X), Math.Max(box2.Min.Y, other.Min.Y),
            Math.Min(box2.Min.X, other.Min.X), Math.Min(box2.Min.Y, other.Min.Y));
}
