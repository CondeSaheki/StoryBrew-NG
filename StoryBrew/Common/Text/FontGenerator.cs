using SkiaSharp;

namespace StoryBrew.Common.Text;

public class FontGenerator
{
    public string Directory { get; } = string.Empty;
    public FontDescription Description { get; } = new();
    public IFontEffect[] Effects { get; } = [];

    private const string file_extention = ".png";
    private const SKEncodedImageFormat extention = SKEncodedImageFormat.Png;

    public FontGenerator(string directory, FontDescription description, IFontEffect[] effects)
    {
        Directory = directory;
        Description = description;
        Effects = effects;
    }

    public FontTexture? GetTexture(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var name = hash(text);

        var filePath = Path.Combine(Directory, name + file_extention);
        if (!File.Exists(filePath)) return createTexture(text, filePath);

        return new FontTexture(filePath, 0, 0, 0, 0, 0, 0);
    }

    /// <summary>
    /// A simple fast hash function not cryptographically secure.
    /// Colisions chance is not tested if you get issues please report.
    /// </summary>
    private static string hash(string text)
    {
        const int shift = 5;

        if (text.Length == 1) return $"{(int)text[0]:x8}"; // example: "a" -> "00000041"

        int length = text.Length;
        int hash = length;
        foreach (var character in text)
        {
            hash ^= character;
            hash = (hash << shift) | (hash >> (32 - shift));
        }
        return $"_{hash:x8}"; // example: "abc" -> "_30c2407f"
    }

    private FontTexture createTexture(string text, string bitmapPath)
    {
        if (!File.Exists(Description.FontPath)) throw new Exception($"Font file {Description.FontPath} do not exist.");

        System.IO.Directory.CreateDirectory(Directory);

        float offsetX = 0, offsetY = 0;
        int baseWidth, baseHeight, width, height;

        using (var typeface = File.Exists(Description.FontPath) ? SKTypeface.FromFile(Description.FontPath) : SKTypeface.Default)
        {
            using (var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = Description.FontSize,
                Color = new SKColor((byte)(Description.Color.R * 255), (byte)(Description.Color.G * 255),
                    (byte)(Description.Color.B * 255), (byte)(Description.Color.A * 255)),
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                baseWidth = (int)Math.Ceiling(textBounds.Width);
                baseHeight = (int)Math.Ceiling(textBounds.Height);

                var effectsWidth = 0f;
                var effectsHeight = 0f;
                foreach (var effect in Effects)
                {
                    var effectSize = effect.Measure();
                    effectsWidth = Math.Max(effectsWidth, effectSize.X);
                    effectsHeight = Math.Max(effectsHeight, effectSize.Y);
                }
                width = (int)Math.Ceiling(baseWidth + effectsWidth + Description.Padding.X * 2);
                height = (int)Math.Ceiling(baseHeight + effectsHeight + Description.Padding.Y * 2);

                var paddingX = Description.Padding.X + effectsWidth * 0.5f;
                var paddingY = Description.Padding.Y + effectsHeight * 0.5f;
                var textX = paddingX;
                var textY = paddingY - textBounds.Top; // Adjust for baseline

                offsetX = -paddingX;
                offsetY = -paddingY;

                if (text.Length == 1 && char.IsWhiteSpace(text[0]) || width == 0 || height == 0)
                    return new FontTexture(null, offsetX, offsetY, baseWidth, baseHeight, width, height);

                using (var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul))
                {
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.Clear(SKColors.Transparent);

                        foreach (var effect in Effects)
                            if (!effect.Overlay)
                                effect.Draw(bitmap, canvas, paint, text, textX, textY);

                        if (!Description.EffectsOnly)
                            canvas.DrawText(text, textX, textY, paint);

                        foreach (var effect in Effects)
                            if (effect.Overlay)
                                effect.Draw(bitmap, canvas, paint, text, textX, textY);
                    }

                    if (Description.TrimTransparency)
                    {
                        var bounds = findTransparencyBounds(bitmap);
                        if (bounds != null && bounds != new SKRectI(0, 0, bitmap.Width, bitmap.Height))
                        {
                            offsetX += bounds.Value.Left;
                            offsetY += bounds.Value.Top;
                            width = (int)bounds.Value.Width;
                            height = (int)bounds.Value.Height;

                            using (var trimmedBitmap = new SKBitmap(width, height))
                            {
                                using (var canvas = new SKCanvas(trimmedBitmap))
                                    canvas.DrawBitmap(bitmap, new SKRectI((int)bounds.Value.Left,
                                        (int)bounds.Value.Top, (int)bounds.Value.Right, (int)bounds.Value.Bottom),
                                        new SKRectI(0, 0, width, height));
                                saveBitmap(trimmedBitmap, bitmapPath);
                            }
                        }
                        else saveBitmap(bitmap, bitmapPath);
                    }
                    else saveBitmap(bitmap, bitmapPath);
                }
            }
        }
        return new FontTexture(Path.Combine(Directory, bitmapPath), offsetX, offsetY, baseWidth, baseHeight, width, height);
    }

    private static SKRect? findTransparencyBounds(SKBitmap source)
    {
        int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;
        bool foundPixel = false;

        using (var pixmap = source.PeekPixels())
        {
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var pixel = pixmap.GetPixelColorF(x, y);
                    if (pixel.Alpha != 0)
                    {
                        foundPixel = true;
                        xMin = Math.Min(xMin, x);
                        xMax = Math.Max(xMax, x);
                        yMin = Math.Min(yMin, y);
                        yMax = Math.Max(yMax, y);
                    }
                }
            }
        }

        if (!foundPixel) return null;

        return new SKRect(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
    }

    private static void saveBitmap(SKBitmap bitmap, string path)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(extention, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }
}
