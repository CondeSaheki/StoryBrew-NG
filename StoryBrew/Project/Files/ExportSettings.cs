/*
    Note: Obsolete, To be removed/replaced in next update.

    Todo: ProjectConfiguration or commandline arguments might be more apropriate for this configuration.
*/

using System.Globalization;

namespace StoryBrew.Project.Files;

[Obsolete("To be removed/replaced in next update.")]
public class ExportSettings
{
    /// <summary>
    /// Not compatible with Fallback!
    /// </summary>
    public bool UseFloatForMove = true;

    /// <summary>
    /// Not compatible with Stable!
    /// </summary>
    public bool UseFloatForTime = false;

    /// <summary>
    /// Enables optimisation for OsbSprites that have a MaxCommandCount > 0
    /// </summary>
    public bool OptimiseSprites = true;

    public readonly NumberFormatInfo NumberFormat = new CultureInfo(@"en-US", false).NumberFormat;

    [Obsolete("To be removed/replaced in next update.")]
    public ExportSettings() { }
}
