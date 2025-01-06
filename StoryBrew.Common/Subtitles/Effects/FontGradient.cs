using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Subtitles;

public class FontGradient : FontEffect
{
    public Vector2 Offset = new(0, 0);
    public Vector2 Size = new(0, 24);
    public Color4 Color = new(255, 0, 0, 0);
    // public WrapMode WrapMode = WrapMode.TileFlipXY;

    public bool Overlay => true;
    public Vector2 Measure() => Vector2.Zero;

    public void Draw(SKBitmap bitmap, SKCanvas canvas, SKPaint paint, string text, float x, float y)
    {
        throw new NotImplementedException();
        // var transparentColor = Color.WithOpacity(0);
        // using (var brush = new LinearGradientBrush(
        //     new PointF(x + Offset.X, y + Offset.Y),
        //     new PointF(x + Offset.X + Size.X, y + Offset.Y + Size.Y),
        //     System.Drawing.Color.FromArgb(Color.ToArgb()),
        //     System.Drawing.Color.FromArgb(transparentColor.ToArgb()))
        //     { WrapMode = WrapMode, })
        //     textGraphics.DrawString(text, font, brush, x, y, stringFormat);
    }
}
