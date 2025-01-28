
// Note: Osu video element are not documented and need to be tested extensively or reverse-engineered

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

    internal override void Write(uint depth = 0)
    {
        const Layer layer = Layer.Video;

        const string identifier = "Video";

        var indentation = new string(' ', (int)depth);

        var result = $"{indentation}{identifier},{layer},{Origin},\"{FilePath}\",{InitialPosition.X},{InitialPosition.Y},{Offset}";
    }
}
