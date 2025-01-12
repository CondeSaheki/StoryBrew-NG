using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Text;

public interface IFontEffect
{
    bool Overlay { get; }

    Vector2 Measure();

    void Draw(SKBitmap bitmap, SKCanvas canvas, SKPaint paint, string text, float x, float y);
}
