using System.Text;
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding;

public class Animation : Commandable
{
    public int FrameCount;
    public double FrameDelay;
    public bool LoopForever;

    public Animation(string path, Anchor origin, Vector2 initialPosition, int frameCount, double frameDelay, bool loopForever)
        : base(path, origin, initialPosition)
    {
        FrameCount = frameCount;
        FrameDelay = frameDelay;
        LoopForever = loopForever;
    }

    public int GetFrameAt(double time)
    {
        var frame = (time - StartTime) / FrameDelay;

        if (LoopForever) frame %= FrameCount;
        else frame = Math.Min(frame, FrameCount - 1);

        return Math.Max(0, (int)frame);
    }

    public override string ToString() => $"Animation -> {StartTime} -> {EndTime}, {Origin} {InitialPosition}, {FrameCount} {FrameDelay} {(LoopForever ? "Forever" : "Once")}";

    internal override void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0)
    {
        const string identifier = "Animation";

        var result = $"{identifier},{layer},{Origin},\"{FilePath}\",{InitialPosition.X},{InitialPosition.Y},{FrameCount},{FrameDelay},{LoopForever}";

        writer.AppendLine(result);

        base.Write(log, writer, layer, depth + 1);
    }
}
