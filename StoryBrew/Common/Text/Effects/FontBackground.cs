using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Text;

public class FontBackground : IFontEffect
{
    public Color4 Color = new(0, 0, 0, 255);

    public bool Overlay => false;
    public Vector2 Measure() => Vector2.Zero;

    public void Draw(FontGenerator generator, SKCanvas canvas, SKTextBlob text)
    {
        throw new NotImplementedException();

        // textGraphics.Clear(System.Drawing.Color.FromArgb(Color.ToArgb()));
    }
}
