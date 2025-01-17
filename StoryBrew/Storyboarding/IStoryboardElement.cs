using StoryBrew.Storyboarding.Commands;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public interface IStoryboardElement
{
    // string Path { get; }
    double StartTime { get; }
    void WriteOsb(TextWriter writer, ExportSettings exportSettings, Layer layer, StoryboardTransform? transform);
}

public interface ICommandableStoryboardElement : IStoryboardElement
{
    IEnumerable<ICommand> Commands { get; set; }
}
