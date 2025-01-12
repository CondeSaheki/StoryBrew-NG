using StoryBrew.Storyboarding;
using OpenTK.Mathematics;

namespace StoryBrew.Common.Text;

public class FontTexture
{
    public string? Path { get; }
    public bool IsEmpty => Path == null;
    public float OffsetX { get; }
    public float OffsetY { get; }
    public int BaseWidth { get; }
    public int BaseHeight { get; }
    public int Width { get; }
    public int Height { get; }

    public FontTexture(string? path, float offsetX, float offsetY, int baseWidth, int baseHeight, int width, int height)
    {
        Path = path;
        OffsetX = offsetX;
        OffsetY = offsetY;
        BaseWidth = baseWidth;
        BaseHeight = baseHeight;
        Width = width;
        Height = height;
    }

    public Vector2 OffsetFor(OsbOrigin origin)
    {
        switch (origin)
        {
            default:
            case OsbOrigin.TopLeft: return new(OffsetX, OffsetY);
            case OsbOrigin.TopCentre: return new(OffsetX + Width * 0.5f, OffsetY);
            case OsbOrigin.TopRight: return new(OffsetX + Width, OffsetY);
            case OsbOrigin.CentreLeft: return new(OffsetX, OffsetY + Height * 0.5f);
            case OsbOrigin.Centre: return new(OffsetX + Width * 0.5f, OffsetY + Height * 0.5f);
            case OsbOrigin.CentreRight: return new(OffsetX + Width, OffsetY + Height * 0.5f);
            case OsbOrigin.BottomLeft: return new(OffsetX, OffsetY + Height);
            case OsbOrigin.BottomCentre: return new(OffsetX + Width * 0.5f, OffsetY + Height);
            case OsbOrigin.BottomRight: return new(OffsetX + Width, OffsetY + Height);
        }
    }
}
