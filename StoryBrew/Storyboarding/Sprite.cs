
using System.Globalization;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding;

public class Sprite : Commandable
{
    public Sprite(string path, Anchor origin, Vector2 initialPosition) : base(path, origin, initialPosition) { }

    public override string ToString() => $"Sprite -> {StartTime} -> {EndTime}, {Origin} {InitialPosition}";

    internal override void Write(StreamWriter writer, uint depth = 0)
    {
        const Layer layer = Layer.Background;

        const string identifier = "Sprite";

        var indentation = new string(' ', (int)depth);

        var result = $"{indentation}{identifier},{layer},{Origin},\"{FilePath}\",{InitialPosition.X},{InitialPosition.Y}";

        writer.WriteLine(result);

        base.Write(writer, depth + 1);
    }
}
