using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Text;

public class FontShadow : IFontEffect
{
    public int Thickness = 1;
    public Color4 Color = new(0, 0, 0, 100);

    public bool Overlay => false;

    // Measure the shadow's effect size (based on Thickness)
    public Vector2 Measure() => new Vector2(Thickness * 2);

    // Draw the shadow effect using SkiaSharp
    public void Draw(FontGenerator generator, SKCanvas canvas, SKTextBlob text)
    {
        throw new NotImplementedException();
        // if (Thickness < 1)
        //     return;

        // // Create a new paint for shadow with the desired color
        // using (var shadowPaint = new SKPaint(paint))
        // {
        //     shadowPaint.Color = new SKColor((byte)(Color.R * 255), (byte)(Color.G * 255),
        //         (byte)(Color.B * 255), (byte)(Color.A * 255));
        //     shadowPaint.IsAntialias = true;

        //     // Draw shadow with the specified Thickness
        //     for (var i = 1; i <= Thickness; i++)
        //     {
        //         // Offset shadow slightly for each step
        //         canvas.DrawText(text, x + i, y + i, shadowPaint);
        //     }
        // }
    }
}
