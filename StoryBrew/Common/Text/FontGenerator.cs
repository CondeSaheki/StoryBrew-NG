using OpenTK.Mathematics;
using SkiaSharp;

namespace StoryBrew.Common.Text;

public class FontGenerator : IDisposable
{
    public string Directory { get; set; }
    public SKFont Font { get; set; }
    public SKPaint Paint { get; set; }

    public IFontEffect[] Effects { get; set; } = [];
    public Vector2 Padding { get; set; } = Vector2.Zero;
    public bool TrimTransparency { get; set; } = true;
    public bool EffectsOnly { get; set; } = false;

    public FontGenerator(string path, SKTypeface typeface, SKPaint paint) : this(path, typeface.ToFont(), paint) { }

    public FontGenerator(string path, SKFont font, SKPaint paint)
    {
        Directory = path;
        Font = font;
        Paint = paint;
    }

    /// <summary>
    /// Returns a SKBitmap for the given text, or null if the text is null or whitespace.
    /// If the bitmap does not exist, it will be created and saved to the directory with the given name.
    /// </summary>
    /// <param name="text">The text to get the bitmap for.</param>
    /// <returns>The SKBitmap for the given text, or null if the text is null or whitespace.</returns>
    public SKBitmap? GetBitmap(string? text)
    {
        const string file_extention = ".png";
        const SKEncodedImageFormat extention = SKEncodedImageFormat.Png;

        if (string.IsNullOrWhiteSpace(text)) return null;

        var name = hash(text);

        var filePath = Path.Combine(Directory, name + file_extention);
        if (!File.Exists(filePath))
        {
            var bitmap = createBitmap(text);
            if (bitmap != null) saveBitmap(bitmap, filePath, extention);
            return bitmap;
        }

        return SKBitmap.Decode(filePath);
    }

    /// <summary>
    /// Computes a simple fast not cryptographically secure hash for the given text.
    /// The probability of collisions has not been tested. If you encounter issues, please report them.
    /// </summary>
    /// <param name="text">The input text to hash.</param>
    /// <returns>A string representing the hash value of the input text in hexadecimal format, prefixed with an underscore.</returns>
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

    /// <summary>
    /// Creates a Bitmap for the given text using the specified font and paint.
    /// </summary>
    /// <param name="text">The text to create a bitmap with.</param>
    /// <returns>A SKBitmap with the text.</returns>
    /// <exception cref="Exception">Thrown if the text blob creation fails.</exception>
    private SKBitmap createBitmap(string text)
    {
        using var blob = SKTextBlob.Create(text, Font) ?? throw new Exception($"Failed to create text blob with {text}");

        var bitmap = new SKBitmap((int)Math.Ceiling(blob.Bounds.Width), (int)Math.Ceiling(blob.Bounds.Height), true);

        using (var canvas = new SKCanvas(bitmap))
        {

            foreach (var effect in Effects)
            {
                if (effect.Overlay) continue;
                effect.Draw(this, canvas, blob);
            }

            if (!EffectsOnly) canvas.DrawText(blob, 0, 0, Paint);

            foreach (var effect in Effects)
            {
                if (!effect.Overlay) continue;
                effect.Draw(this, canvas, blob);
            }

        }

        return bitmap;
    }

    /// <summary>
    /// Saves the given bitmap to the specified file path as an encoded image.
    /// </summary>
    /// <param name="bitmap">The SKBitmap to save.</param>
    /// <param name="filePath">The file path where the bitmap will be saved.</param>
    /// <param name="extention">The encoding format to use when saving the bitmap.</param>
    /// <exception cref="ArgumentNullException">Thrown if the bitmap or filePath is null.</exception>
    /// <exception cref="IOException">Thrown if there is an error saving the bitmap to the file path.</exception>
    private static void saveBitmap(SKBitmap bitmap, string filePath, SKEncodedImageFormat extention = SKEncodedImageFormat.Png)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(extention, 100);
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(stream);
    }

    public void Dispose()
    {
        Font?.Dispose();
        Paint?.Dispose();
    }
}

/*

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
                            if (!effect.Overlay) effect.Draw(bitmap, canvas, paint, text, textX, textY);

                        if (!Description.EffectsOnly) canvas.DrawText(text, textX, textY, paint);

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

*/
