using SkiaSharp;

namespace StoryBrew.Common.Util;
internal static class BitmapHelper
{
    public static SKBitmap Blur(SKBitmap source, int radius)
    {
        var paint = new SKPaint
        {
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, radius)
        };

        var result = new SKBitmap(source.Width, source.Height);
        using (var canvas = new SKCanvas(result))
        {
            canvas.DrawBitmap(source, 0, 0, paint);
        }

        return result;
    }

    public static SKBitmap Premultiply(SKBitmap source)
    {
        var result = new SKBitmap(source.Width, source.Height);

        result.Pixels = source.Pixels.Select((color) =>
        {
            float alphaFactor = color.Alpha / 255f;
            return new SKColor(
                (byte)(color.Red * alphaFactor),
                (byte)(color.Green * alphaFactor),
                (byte)(color.Blue * alphaFactor),
                color.Alpha);
        }).ToArray();

        return result;
    }

    public static double[,] CalculateGaussianKernel(int radius, double weight)
    {
        var length = radius * 2 + 1;
        var kernel = new double[length, length];
        var total = 0.0;

        var scale = 1.0 / (2.0 * Math.PI * Math.Pow(weight, 2));
        for (var y = -radius; y <= radius; y++)
            for (var x = -radius; x <= radius; x++)
            {
                var distance = (x * x + y * y) / (2 * weight * weight);
                var value = kernel[y + radius, x + radius] = scale * Math.Exp(-distance);
                total += value;
            }

        for (var y = 0; y < length; y++)
            for (var x = 0; x < length; x++)
                kernel[y, x] = kernel[y, x] / total;

        return kernel;
    }

    public static SKBitmap Convolute(SKBitmap source, double[,] kernel)
    {
        var kernelHeight = kernel.GetUpperBound(0) + 1;
        var kernelWidth = kernel.GetUpperBound(1) + 1;

        if ((kernelWidth % 2) == 0 || (kernelHeight % 2) == 0)
            throw new InvalidOperationException("Invalid kernel size");

        var width = source.Width;
        var height = source.Height;
        var result = new SKBitmap(width, height);

        var halfKernelWidth = kernelWidth / 2;
        var halfKernelHeight = kernelHeight / 2;

        using (var pixmap = source.PeekPixels())
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = 0, r = 0, g = 0, b = 0;

                    for (int kernelY = -halfKernelHeight; kernelY <= halfKernelHeight; kernelY++)
                    {
                        for (int kernelX = -halfKernelWidth; kernelX <= halfKernelWidth; kernelX++)
                        {
                            var pixelX = Math.Min(Math.Max(x + kernelX, 0), width - 1);
                            var pixelY = Math.Min(Math.Max(y + kernelY, 0), height - 1);

                            var pixel = pixmap.GetPixelColorF(pixelX, pixelY);

                            var kernelValue = kernel[kernelY + halfKernelHeight, kernelX + halfKernelWidth];
                            a += pixel.Alpha * kernelValue;
                            r += pixel.Red * kernelValue;
                            g += pixel.Green * kernelValue;
                            b += pixel.Blue * kernelValue;
                        }
                    }

                    // Normalize the pixel values and clamp them between 0-255
                    var alpha = (byte)Math.Clamp((int)a, 0, 255);
                    var red = (byte)Math.Clamp((int)r, 0, 255);
                    var green = (byte)Math.Clamp((int)g, 0, 255);
                    var blue = (byte)Math.Clamp((int)b, 0, 255);

                    result.SetPixel(x, y, new SKColor(red, green, blue, alpha));
                }
            }
        }

        return result;
    }

    public static SKBitmap ConvoluteAlpha(SKBitmap source, double[,] kernel, SKColor color)
    {
        var kernelHeight = kernel.GetUpperBound(0) + 1;
        var kernelWidth = kernel.GetUpperBound(1) + 1;

        if ((kernelWidth % 2) == 0 || (kernelHeight % 2) == 0)
            throw new InvalidOperationException("Invalid kernel size");

        var width = source.Width;
        var height = source.Height;
        var result = new SKBitmap(width, height);

        var halfKernelWidth = kernelWidth / 2;
        var halfKernelHeight = kernelHeight / 2;

        using (var pixmap = source.PeekPixels())
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = 0;

                    for (int kernelY = -halfKernelHeight; kernelY <= halfKernelHeight; kernelY++)
                    {
                        for (int kernelX = -halfKernelWidth; kernelX <= halfKernelWidth; kernelX++)
                        {
                            var pixelX = Math.Min(Math.Max(x + kernelX, 0), width - 1);
                            var pixelY = Math.Min(Math.Max(y + kernelY, 0), height - 1);

                            var pixel = pixmap.GetPixelColorF(pixelX, pixelY);

                            var kernelValue = kernel[kernelY + halfKernelHeight, kernelX + halfKernelWidth];
                            a += pixel.Alpha * kernelValue;
                        }
                    }

                    // Normalize the alpha value
                    var alpha = (byte)Math.Clamp((int)a, 0, 255);

                    result.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, alpha));
                }
            }
        }

        return result;
    }

    public static SKRect? FindTransparencyBounds(SKBitmap source)
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
}
