
using OpenTK.Mathematics;
using StoryBrew.Storyboard.Common;

namespace StoryBrew.Storyboard.Core;

public abstract class ElementTransformable : Transformable, IElement
{
    public string FilePath { get; }
    public Anchor Origin;
    public Vector2 InitialPosition;

    public ElementTransformable(string filePath, Anchor origin, Vector2 initialPosition) : base()
    {
        FilePath = filePath;
        Origin = origin;
        InitialPosition = initialPosition;
    }

    // internal override void Write(uint depth = 0) => base.Write(depth);
}
