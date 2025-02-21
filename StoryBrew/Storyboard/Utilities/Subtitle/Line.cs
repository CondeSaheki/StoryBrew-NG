namespace StoryBrew.Storyboard.Utilities.Subtitle;

public class Line
{
    public double StartTime { get; set; } = 0;
    public double EndTime { get; set; } = 0;
    public string Text { get; set; } = string.Empty;

    public Line() { }

    public Line(double startTime, double endTime, string text)
    {
        StartTime = startTime;
        EndTime = endTime;
        Text = text;
    }
}
