using OpenTK.Mathematics;
using SkiaSharp;

public static class SKColorExtensions
{
    public static Color4 ToColor4(this SKColor color) => new(color.Red, color.Green, color.Blue, color.Alpha);
}
