
using System.Globalization;
using System.Text;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding;

public class Sprite : Commandable
{
    public Sprite(string path, Anchor origin, Vector2 initialPosition) : base(path, origin, initialPosition) { }

    public override string ToString() => $"Sprite -> {StartTime} -> {EndTime}, {Origin} {InitialPosition}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "Sprite";

        var indentation = new string(' ', (int)depth);

        var result = $"{indentation}{identifier},{layer},{Origin},\"{FilePath}\",{InitialPosition.X},{InitialPosition.Y}";

        writer.AppendLine(result);

        base.Write(log, writer, layer, depth + 1);
    }
}
