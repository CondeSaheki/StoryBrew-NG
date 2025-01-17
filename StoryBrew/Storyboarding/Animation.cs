namespace StoryBrew.Storyboarding;

public class Animation : Sprite
{
    public int FrameCount;
    public double FrameDelay;
    public LoopType LoopType;
    public double LoopDuration => FrameCount * FrameDelay;
    public double AnimationEndTime => (LoopType == LoopType.LoopOnce) ? StartTime + LoopDuration : EndTime;

    public override string GetTexturePathAt(double time)
    {
        var dotIndex = Path.LastIndexOf('.');
        if (dotIndex < 0) return Path + GetFrameAt(time);

        return Path[..dotIndex] + GetFrameAt(time) + Path[dotIndex..];
    }

    public int GetFrameAt(double time)
    {
        var frame = (time - CommandsStartTime) / FrameDelay;
        switch (LoopType)
        {
            case LoopType.LoopForever:
                frame %= FrameCount;
                break;
            case LoopType.LoopOnce:
                frame = Math.Min(frame, FrameCount - 1);
                break;
        }
        return Math.Max(0, (int)frame);
    }
}
