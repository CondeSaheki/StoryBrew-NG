namespace StoryBrew.Storyboard.Core;

public interface ITransform // : IComparable<ICommand>
{
    double StartTime { get; }
    double EndTime { get; }
}
