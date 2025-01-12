namespace StoryBrew.Common.Subtitles;

public class SubtitleLine
{
    public double StartTime { get; set; } = 0;
    public double EndTime { get; set; } = 0;
    public string Text { get; set; } = string.Empty;

    public SubtitleLine() { }

    public SubtitleLine(double startTime, double endTime, string text)
    {
        StartTime = startTime;
        EndTime = endTime;
        Text = text;
    }
}
