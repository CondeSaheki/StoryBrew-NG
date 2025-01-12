using OpenTK.Mathematics;

namespace StoryBrew.Common.Text;

public class FontDescription
{
    public string FontPath = string.Empty;
    public int FontSize = 76;
    public Color4 Color = new(0, 0, 0, 100);
    public Vector2 Padding = Vector2.Zero;
    // public FontStyle FontStyle = FontStyle.Regular;
    public bool TrimTransparency;
    public bool EffectsOnly;
    public bool Debug;
}
