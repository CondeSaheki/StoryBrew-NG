namespace StoryBrew.Storyboarding;

public abstract class Writable
{
    internal abstract void Write(StreamWriter writer, uint depth = 0);
}
