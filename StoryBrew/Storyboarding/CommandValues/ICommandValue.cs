using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding.CommandValues;

public interface ICommandValue
{
    float DistanceFrom(object obj);
    string ToOsbString(ExportSettings exportSettings);
}
