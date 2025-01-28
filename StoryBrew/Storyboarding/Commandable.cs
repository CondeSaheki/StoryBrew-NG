
using OpenTK.Mathematics;

namespace StoryBrew.Storyboarding;

public abstract class Commandable : Group, IElement
{
    public string FilePath { get; }
    public Anchor Origin;
    public Vector2 InitialPosition;

    public Commandable(string filePath, Anchor origin, Vector2 initialPosition) : base()
    {
        FilePath = filePath;
        Origin = origin;
        InitialPosition = initialPosition;
    }

    // internal override void Write(uint depth = 0) => base.Write(depth);
}
