using System.Text;
using StoryBrew.Storyboarding.Commands;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public class Container : IStoryboardElement
{
    public string Path => throw new NotImplementedException();
    public double StartTime => throw new NotImplementedException();

    public double EndTime => throw new NotImplementedException();

    public List<ICommandableStoryboardElement> Elements = [];

    public List<ICommand> Commands = [];

    public Container(List<ICommandableStoryboardElement> elements)
    {
        Elements = elements;
    }

    public void WriteOsb(TextWriter writer, ExportSettings exportSettings, Layer layer, StoryboardTransform? transform)
    {
        throw new NotImplementedException();

        // var builder = new StringBuilder();

        // foreach (var element in Elements)
        // {
        //     element.Commands = combine([.. element.Commands]);
        //     builder.AppendLine(element.WriteOsb());
        // }
        // var result = builder.ToString();
    }

    private List<ICommand> combine(List<ICommand> elementCommands)
    {
        throw new NotImplementedException();

        // List<ICommand> combined = [.. Commands, .. elementCommands];
        // combined.Sort();

        // return combined;
    }

    public void Move() => throw new NotImplementedException();

    public void Scale() => throw new NotImplementedException();

    public void Rotate() => throw new NotImplementedException();

    public void VectorScale() => throw new NotImplementedException();

}
