using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Text;

public interface IFontEffect
{
    public bool Overlay { get; }

    public Vector2 Measure();

    public void Draw(FontGenerator generator, SKCanvas canvas, SKTextBlob text);
}
