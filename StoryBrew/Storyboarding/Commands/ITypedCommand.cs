namespace StoryBrew.Storyboarding.Commands;

public interface ITypedCommand<TValue> : ICommand
{
    Easing Easing { get; }
    TValue StartValue { get; }
    TValue EndValue { get; }
    double Duration { get; }

    TValue ValueAtTime(double time);
}
