namespace StoryBrew.Subtitles;

public class SubtitleSet
{
    public IEnumerable<SubtitleLine> Lines {get; private set; }

    public SubtitleSet(IEnumerable<SubtitleLine> lines)
    {
        Lines = [.. lines];
    }
}
