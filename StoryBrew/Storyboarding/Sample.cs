namespace StoryBrew.Storyboarding;

public class Sample : Writable, IElement
{
    public string Path { get; }
    public double StartTime { get; }
    public int Volume { get; }

    public Sample(string path, double startTime, int volume)
    {
        Path = path;
        StartTime = startTime;
        Volume = volume;
    }

    public override string ToString() => $"Sample -> {StartTime} {Volume}";

    internal override void Write(StreamWriter writer, uint depth = 0)
    {
        const Layer layer = Layer.Background;

        const string identifier = "Sample";

        var indentation = new string(' ', (int)depth);

        var result = $"{indentation}{identifier},{StartTime},{layer},\"{Path}\",{Volume}";

        writer.WriteLine(result);
    }
}
