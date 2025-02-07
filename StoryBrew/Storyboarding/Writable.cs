using System.Text;

namespace StoryBrew.Storyboarding;

public abstract class Writable
{
    internal abstract void Write(StringBuilder log, StringBuilder writer, Layer layer, uint depth = 0);
}
