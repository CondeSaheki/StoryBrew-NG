namespace StoryBrew.Storyboarding;

public interface ICommand // : IComparable<ICommand>
{
    double StartTime { get; }
    double EndTime { get; }
}
