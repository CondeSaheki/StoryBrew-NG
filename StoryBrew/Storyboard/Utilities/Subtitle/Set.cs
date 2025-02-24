namespace StoryBrew.Storyboard.Utilities.Subtitle;

public class Set
{
    public IEnumerable<Line> Lines { get; private set; } = [];

    public Set() { }

    public Set(IEnumerable<Line> lines)
    {
        Lines = lines;
    }
}
