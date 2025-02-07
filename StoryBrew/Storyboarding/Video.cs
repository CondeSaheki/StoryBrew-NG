
// Note: Osu video element are not documented and need to be tested extensively or reverse-engineered

using System.Text;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding;

public class Video : Commandable
{
    public double Offset { get; }

    public Video(string path, double offset) : base(path, Anchor.Centre, Vector2.Zero)
    {
        Offset = offset;
        throw new NotImplementedException();
    }

    public override string ToString() => $"Video -> {StartTime} -> {EndTime}, {Origin} {InitialPosition}, {Offset}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        if (layer != Layer.Video) throw new InvalidOperationException("Video element must be in Video layer.");

        const string identifier = "Video";

        var indentation = new string(' ', (int)depth);

        var result = $"{indentation}{identifier},{layer},{Origin},\"{FilePath}\",{InitialPosition.X},{InitialPosition.Y},{Offset}";

        writer.AppendLine(result);
    }
}
