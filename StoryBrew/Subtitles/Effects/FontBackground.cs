using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Subtitles;

public class FontBackground : FontEffect
{
    public Color4 Color = new(0, 0, 0, 255);

    public bool Overlay => false;
    public Vector2 Measure() => Vector2.Zero;

    public void Draw(SKBitmap bitmap, SKCanvas canvas, SKPaint paint, string text, float x, float y)
    {
        throw new NotImplementedException();

        // textGraphics.Clear(System.Drawing.Color.FromArgb(Color.ToArgb()));
    }
}
