/*
    Todo: this logic was taken from other project and needs to be refactored, it can be implemented as
    extentions methods or at least have more appropriated naming
*/

using OpenTK.Mathematics;

namespace StoryBrew.Common;

public static partial class Converter
{
    private const float default_screen_width = 1920;
    private const float default_screen_height = 1080;
    private const float osu_width = 640;
    private const float osu_height = 480;

    [Obsolete("To be refactored")]
    public static Vector2 ScreenToOsu(float x, float y, (float, float)? screen = default)
    {
        (float width, float height) = screen ?? (default_screen_width, default_screen_height);

        var xOsu = (osu_height * x - (osu_height / 2) * width + (osu_width / 2) * height) / height;
        var yOsu = y * osu_height / height;
        return new Vector2(xOsu, yOsu);
    }

    [Obsolete("To be refactored")]
    public static Vector2 ScreenToOsu(Vector2 position, (float, float)? screen = default) => ScreenToOsu(position.X, position.Y, screen);

    [Obsolete("To be refactored")]
    public static float PixelToOsu(int pixels, (float, float)? screen = default)
    {
        float height = screen?.Item2 ?? default_screen_height;

        return pixels * osu_height / height;
    }

    [Obsolete("To be refactored")]
    public static Vector2 PixelToOsu(int x, int y, (float, float)? screen = default) => new(PixelToOsu(x, screen), PixelToOsu(y, screen));

    [Obsolete("To be refactored")]
    public static Vector2 PixelToOsu(Vector2 size, (float, float)? screen = default) => PixelToOsu((int)size.X, (int)size.Y, screen);

    [Obsolete("To be refactored")]
    public static Vector2 OsuToScreen(float x, float y, (float, float)? screen = default)
    {
        (float width, float height) = screen ?? (default_screen_width, default_screen_height);

        var xScreen = (x * height + (osu_height / 2) * width - (osu_width / 2) * height) / osu_height;
        var yScreen = y * height / osu_height;
        return new Vector2(xScreen, yScreen);
    }

    [Obsolete("To be refactored")]
    public static Vector2 OsuToScreen(Vector2 position, (float, float)? screen = default) => OsuToScreen(position.X, position.Y, screen);

    [Obsolete("To be refactored")]
    public static float OsuToPixel(float value, (float, float)? screen = default)
    {
        float height = screen?.Item2 ?? default_screen_height;

        return value * height / osu_height;
    }
}
