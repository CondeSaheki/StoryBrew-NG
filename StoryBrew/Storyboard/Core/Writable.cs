// Note: This was made an abstract class to allow hiding the write method setting it as internal

using System.Text;
using StoryBrew.Storyboard.Common;

namespace StoryBrew.Storyboard.Core;

public abstract class Writable
{
    internal abstract void Write(StringBuilder writer, Layer layer, uint depth = 0);
}
