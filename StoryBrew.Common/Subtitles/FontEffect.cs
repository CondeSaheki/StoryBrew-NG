using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Subtitles;

public interface FontEffect
{
    bool Overlay { get; }

    Vector2 Measure();

    void Draw(SKBitmap bitmap, SKCanvas canvas, SKPaint paint, string text, float x, float y);
}
