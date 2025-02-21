using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Storyboard.Common.Extensions;

public static class Color4Extensions
{
    public static SKColor ToSKColor(this Color4 color)
    {
        byte R = (byte)Math.Floor(byte.MaxValue * color.R);
        byte G = (byte)Math.Floor(byte.MaxValue * color.G);
        byte B = (byte)Math.Floor(byte.MaxValue * color.B);
        byte A = (byte)Math.Floor(byte.MaxValue * color.A);

        return new(R, G, B, A);
    }
}
