using StoryBrew.Common.Storyboarding;
using StoryBrew.Common.Util;
using System.Diagnostics;
using OpenTK.Mathematics;
using SkiaSharp;
using Tiny;

namespace StoryBrew.Common.Subtitles;

public class FontTexture
{
    public string? Path { get; }
    public bool IsEmpty => Path == null;
    public float OffsetX { get; }
    public float OffsetY { get; }
    public int BaseWidth { get; }
    public int BaseHeight { get; }
    public int Width { get; }
    public int Height { get; }

    public FontTexture(string? path, float offsetX, float offsetY, int baseWidth, int baseHeight, int width, int height)
    {
        Path = path;
        OffsetX = offsetX;
        OffsetY = offsetY;
        BaseWidth = baseWidth;
        BaseHeight = baseHeight;
        Width = width;
        Height = height;
    }

    public Vector2 OffsetFor(OsbOrigin origin)
    {
        switch (origin)
        {
            default:
            case OsbOrigin.TopLeft: return new Vector2(OffsetX, OffsetY);
            case OsbOrigin.TopCentre: return new Vector2(OffsetX + Width * 0.5f, OffsetY);
            case OsbOrigin.TopRight: return new Vector2(OffsetX + Width, OffsetY);
            case OsbOrigin.CentreLeft: return new Vector2(OffsetX, OffsetY + Height * 0.5f);
            case OsbOrigin.Centre: return new Vector2(OffsetX + Width * 0.5f, OffsetY + Height * 0.5f);
            case OsbOrigin.CentreRight: return new Vector2(OffsetX + Width, OffsetY + Height * 0.5f);
            case OsbOrigin.BottomLeft: return new Vector2(OffsetX, OffsetY + Height);
            case OsbOrigin.BottomCentre: return new Vector2(OffsetX + Width * 0.5f, OffsetY + Height);
            case OsbOrigin.BottomRight: return new Vector2(OffsetX + Width, OffsetY + Height);
        }
    }
}

public class FontDescription
{
    public string FontPath = string.Empty;
    public int FontSize = 76;
    public Color4 Color = new Color4(0, 0, 0, 100);
    public Vector2 Padding = Vector2.Zero;
    // public FontStyle FontStyle = FontStyle.Regular;
    public bool TrimTransparency;
    public bool EffectsOnly;
    public bool Debug;
}

public class FontGenerator
{
    public string Directory { get; }
    private readonly FontDescription description;
    private readonly FontEffect[] effects;
    private readonly string projectDirectory;
    private readonly string assetDirectory;

    private readonly Dictionary<string, FontTexture> textureCache = new Dictionary<string, FontTexture>();

    internal FontGenerator(string directory, FontDescription description, FontEffect[] effects, string projectDirectory, string assetDirectory)
    {
        Directory = directory;
        this.description = description;
        this.effects = effects;
        this.projectDirectory = projectDirectory;
        this.assetDirectory = assetDirectory;
    }

    public FontTexture? GetTexture(string text)
    {
        if (!textureCache.TryGetValue(text, out FontTexture? texture))
            textureCache.Add(text, texture = generateTexture(text));
        return texture;
    }

    private FontTexture generateTexture(string text)
    {
        var filename = text.Length == 1 ? $"{(int)text[0]:x4}.png" : $"_{textureCache.Count(l => l.Key.Length > 1):x3}.png";
        var bitmapPath = Path.Combine(assetDirectory, Directory, filename);

        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(bitmapPath) ?? throw new Exception());

        var fontPath = Path.Combine(projectDirectory, description.FontPath);
        if (!File.Exists(fontPath)) fontPath = description.FontPath;

        float offsetX = 0, offsetY = 0;
        int baseWidth, baseHeight, width, height;

        using (var typeface = File.Exists(fontPath) ? SKTypeface.FromFile(fontPath) : SKTypeface.Default)
        {
            using (var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = description.FontSize,
                Color = new SKColor((byte)(description.Color.R * 255), (byte)(description.Color.G * 255),
                    (byte)(description.Color.B * 255), (byte)(description.Color.A * 255)),
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                baseWidth = (int)Math.Ceiling(textBounds.Width);
                baseHeight = (int)Math.Ceiling(textBounds.Height);

                var effectsWidth = 0f;
                var effectsHeight = 0f;
                foreach (var effect in effects)
                {
                    var effectSize = effect.Measure();
                    effectsWidth = Math.Max(effectsWidth, effectSize.X);
                    effectsHeight = Math.Max(effectsHeight, effectSize.Y);
                }
                width = (int)Math.Ceiling(baseWidth + effectsWidth + description.Padding.X * 2);
                height = (int)Math.Ceiling(baseHeight + effectsHeight + description.Padding.Y * 2);

                var paddingX = description.Padding.X + effectsWidth * 0.5f;
                var paddingY = description.Padding.Y + effectsHeight * 0.5f;
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
                        canvas.Clear(description.Debug
                            ? new SKColor((byte)(textureCache.Count * 10 % 256), 128, 128, 255)
                            : SKColors.Transparent);

                        foreach (var effect in effects)
                            if (!effect.Overlay)
                                effect.Draw(bitmap, canvas, paint, text, textX, textY);

                        if (!description.EffectsOnly)
                            canvas.DrawText(text, textX, textY, paint);

                        foreach (var effect in effects)
                            if (effect.Overlay)
                                effect.Draw(bitmap, canvas, paint, text, textX, textY);
                    }

                    if (description.TrimTransparency)
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
                                    canvas.DrawBitmap(bitmap, new SKRectI((int)bounds.Value.Left, (int)bounds.Value.Top, (int)bounds.Value.Right, (int)bounds.Value.Bottom),
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
        return new FontTexture(Path.Combine(Directory, filename), offsetX, offsetY, baseWidth, baseHeight, width, height);
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

    private void saveBitmap(SKBitmap bitmap, string path)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    internal void HandleCache(TinyToken cachedFontRoot)
    {
        if (!matches(cachedFontRoot))
            return;

        foreach (var cacheEntry in cachedFontRoot.Values<TinyObject>("Cache"))
        {
            var path = cacheEntry.Value<string>("Path");
            var hash = cacheEntry.Value<string>("Hash");

            var fullPath = Path.Combine(assetDirectory, path);
            if (!File.Exists(fullPath) || HashHelper.GetFileMd5(fullPath) != hash)
                continue;

            var text = cacheEntry.Value<string>("Text");
            if (text.Contains('\ufffd'))
            {
                Trace.WriteLine($"Ignoring invalid font texture \"{text}\" ({path})");
                continue;
            }
            if (textureCache.ContainsKey(text))
                throw new InvalidDataException($"The font texture for \"{text}\" ({path}) has been cached multiple times");

            textureCache.Add(text, new FontTexture(
                path,
                cacheEntry.Value<float>("OffsetX"),
                cacheEntry.Value<float>("OffsetY"),
                cacheEntry.Value<int>("BaseWidth"),
                cacheEntry.Value<int>("BaseHeight"),
                cacheEntry.Value<int>("Width"),
                cacheEntry.Value<int>("Height")
            ));
        }
    }

    private bool matches(TinyToken cachedFontRoot)
    {
        if (cachedFontRoot.Value<string>("FontPath") == description.FontPath &&
            cachedFontRoot.Value<int>("FontSize") == description.FontSize &&
            floatEquals(cachedFontRoot.Value<float>("ColorR"), description.Color.R, 0.00001f) &&
            floatEquals(cachedFontRoot.Value<float>("ColorG"), description.Color.G, 0.00001f) &&
            floatEquals(cachedFontRoot.Value<float>("ColorB"), description.Color.B, 0.00001f) &&
            floatEquals(cachedFontRoot.Value<float>("ColorA"), description.Color.A, 0.00001f) &&
            floatEquals(cachedFontRoot.Value<float>("PaddingX"), description.Padding.X, 0.00001f) &&
            floatEquals(cachedFontRoot.Value<float>("PaddingY"), description.Padding.Y, 0.00001f) &&
            // cachedFontRoot.Value<FontStyle>("FontStyle") == description.FontStyle &&
            cachedFontRoot.Value<bool>("TrimTransparency") == description.TrimTransparency &&
            cachedFontRoot.Value<bool>("EffectsOnly") == description.EffectsOnly &&
            cachedFontRoot.Value<bool>("Debug") == description.Debug)
        {
            var effectsRoot = cachedFontRoot.Value<TinyArray>("Effects");
            if (effectsRoot.Count != effects.Length)
                return false;

            for (var i = 0; i < effects.Length; i++)
                if (!matches(effects[i], effectsRoot[i].Value<TinyToken>()))
                    return false;

            return true;
        }
        return false;
    }

    private bool matches(FontEffect fontEffect, TinyToken cache)
    {
        var effectType = fontEffect.GetType();
        if (cache.Value<string>("Type") != effectType.FullName)
            return false;

        foreach (var field in effectType.GetFields())
        {
            var fieldType = field.FieldType;
            if (fieldType == typeof(Color4))
            {
                var color = (Color4?)field.GetValue(fontEffect) ?? throw new Exception();
                if (!floatEquals(cache.Value<float>($"{field.Name}R"), color.R, 0.00001f) ||
                    !floatEquals(cache.Value<float>($"{field.Name}G"), color.G, 0.00001f) ||
                    !floatEquals(cache.Value<float>($"{field.Name}B"), color.B, 0.00001f) ||
                    !floatEquals(cache.Value<float>($"{field.Name}A"), color.A, 0.00001f))
                    return false;
            }
            else if (fieldType == typeof(Vector3))
            {
                var vector = (Vector3?)field.GetValue(fontEffect) ?? throw new Exception();
                if (!floatEquals(cache.Value<float>($"{field.Name}X"), vector.X, 0.00001f) ||
                    !floatEquals(cache.Value<float>($"{field.Name}Y"), vector.Y, 0.00001f) ||
                    !floatEquals(cache.Value<float>($"{field.Name}Z"), vector.Z, 0.00001f))
                    return false;
            }
            else if (fieldType == typeof(Vector2))
            {
                var vector = (Vector2?)field.GetValue(fontEffect) ?? throw new Exception();
                if (!floatEquals(cache.Value<float>($"{field.Name}X"), vector.X, 0.00001f) ||
                    !floatEquals(cache.Value<float>($"{field.Name}Y"), vector.Y, 0.00001f))
                    return false;
            }
            else if (fieldType == typeof(double))
            {
                if (!(Math.Abs(cache.Value<double>(field.Name) - (double?)field.GetValue(fontEffect) ?? throw new Exception()) < 0.00001))
                    return false;
            }
            else if (fieldType == typeof(float))
            {
                if (!floatEquals(cache.Value<float>(field.Name), (float?)field.GetValue(fontEffect) ?? throw new Exception(), 0.00001f))
                    return false;
            }
            else if (fieldType == typeof(int) || fieldType.IsEnum)
            {
                if (cache.Value<int>(field.Name) != ((int?)field.GetValue(fontEffect) ?? throw new Exception()))
                    return false;
            }
            else if (fieldType == typeof(string))
            {
                if (cache.Value<string>(field.Name) != ((string?)field.GetValue(fontEffect) ?? throw new Exception()))
                    return false;
            }
            else throw new InvalidDataException($"Unexpected field type {fieldType} for {field.Name} in {effectType.FullName}");
        }
        return true;
    }

    private static bool floatEquals(float a, float b, float epsilon)
        => Math.Abs(a - b) < epsilon;

    internal TinyObject ToTinyObject() => new TinyObject
        {
            { "FontPath", withStandardSeparators(description.FontPath) },
            { "FontSize", description.FontSize },
            { "ColorR", description.Color.R },
            { "ColorG", description.Color.G },
            { "ColorB", description.Color.B },
            { "ColorA", description.Color.A },
            { "PaddingX", description.Padding.X },
            { "PaddingY", description.Padding.Y },
            // { "FontStyle", description.FontStyle },
            { "TrimTransparency", description.TrimTransparency },
            { "EffectsOnly", description.EffectsOnly },
            { "Debug", description.Debug },
            { "Effects", effects.Select(e => fontEffectToTinyObject(e))},
            { "Cache", textureCache.Where(l => !l.Value.IsEmpty).Select(l => letterToTinyObject(l))},
        };

    private TinyObject letterToTinyObject(KeyValuePair<string, FontTexture> letterEntry) => new TinyObject
        {
            { "Text", letterEntry.Key },
            { "Path", withStandardSeparators(letterEntry.Value.Path ?? throw new Exception()) },
            { "Hash", HashHelper.GetFileMd5(Path.Combine(assetDirectory, letterEntry.Value.Path)) },
            { "OffsetX", letterEntry.Value.OffsetX },
            { "OffsetY", letterEntry.Value.OffsetY },
            { "BaseWidth", letterEntry.Value.BaseWidth },
            { "BaseHeight", letterEntry.Value.BaseHeight },
            { "Width", letterEntry.Value.Width },
            { "Height", letterEntry.Value.Height },
        };

    private TinyObject fontEffectToTinyObject(FontEffect fontEffect)
    {
        var effectType = fontEffect.GetType();
        var cache = new TinyObject
        {
            ["Type"] = effectType.FullName,
        };

        foreach (var field in effectType.GetFields())
        {
            var fieldType = field.FieldType;
            if (fieldType == typeof(Color4))
            {
                var color = (Color4?)field.GetValue(fontEffect) ?? throw new Exception();
                cache[$"{field.Name}R"] = color.R;
                cache[$"{field.Name}G"] = color.G;
                cache[$"{field.Name}B"] = color.B;
                cache[$"{field.Name}A"] = color.A;
            }
            else if (fieldType == typeof(Vector3))
            {
                var vector = (Vector3?)field.GetValue(fontEffect) ?? throw new Exception();
                cache[$"{field.Name}X"] = vector.X;
                cache[$"{field.Name}Y"] = vector.Y;
                cache[$"{field.Name}Z"] = vector.Z;
            }
            else if (fieldType == typeof(Vector2))
            {
                var vector = (Vector2?)field.GetValue(fontEffect) ?? throw new Exception();
                cache[$"{field.Name}X"] = vector.X;
                cache[$"{field.Name}Y"] = vector.Y;
            }
            else if (fieldType == typeof(double))
                cache[field.Name] = (double?)field.GetValue(fontEffect) ?? throw new Exception();
            else if (fieldType == typeof(float))
                cache[field.Name] = (float?)field.GetValue(fontEffect) ?? throw new Exception();
            else if (fieldType == typeof(int) || fieldType.IsEnum)
                cache[field.Name] = (int?)field.GetValue(fontEffect) ?? throw new Exception();
            else if (fieldType == typeof(string))
                cache[field.Name] = (string?)field.GetValue(fontEffect) ?? throw new Exception();
            else throw new InvalidDataException($"Unexpected field type {fieldType} for {field.Name} in {effectType.FullName}");
        }

        return cache;
    }

    /// <summary>
    /// Replaces directory separator by a StandardDirectorySeparator
    /// </summary>
    private static string withStandardSeparators(string path)
    {
        const char standard_directory_separator = '/';

        if (Path.DirectorySeparatorChar != standard_directory_separator)
            path = path.Replace(Path.DirectorySeparatorChar, standard_directory_separator);
        path = path.Replace('\\', standard_directory_separator);
        return path;
    }
}
