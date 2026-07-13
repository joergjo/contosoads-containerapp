using SkiaSharp;

namespace ContosoAds.ImageProcessor;

public class ImageProcessor(ILogger<ImageProcessor> logger)
{
    private const int ThumbnailSize = 80;
    private const int JpegQuality = 80;

    public async Task RenderAsync(Stream input, Stream output)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!input.CanRead)
        {
            throw new ArgumentException("Stream must be readable", nameof(input));
        }

        ArgumentNullException.ThrowIfNull(output);

        if (!output.CanWrite)
        {
            throw new ArgumentException("Stream must be writable", nameof(output));
        }

        // SkiaSharp's decode/resize APIs are synchronous, so buffer the input
        // and stream the encoded output asynchronously to keep the public
        // signature unchanged.
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer);
        buffer.Position = 0;

        using var original = SKBitmap.Decode(buffer)
                             ?? throw new InvalidOperationException("Failed to decode input image.");
        var (width, height) = GetEffectiveSize(original.Width, original.Height, ThumbnailSize);

        logger.LogDebug(
            "Resized image from {OriginalWidth}x{OriginalHeight} to {NewWidth}x{NewHeight}",
            original.Width,
            original.Height,
            width,
            height);

        var info = new SKImageInfo(width, height);
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        using var resized = original.Resize(info, sampling)
                            ?? throw new InvalidOperationException("Failed to resize image.");

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        await using var encoded = data.AsStream();
        await encoded.CopyToAsync(output);
    }

    private static (int, int) GetEffectiveSize(int originalWidth, int originalHeight, int newSize)
    {
        int x, y;
        if (originalWidth > originalHeight)
        {
            x = newSize;
            y = newSize * originalHeight / originalWidth;
        }
        else
        {
            y = newSize;
            x = newSize * originalWidth / originalHeight;
        }

        return (x, y);
    }
}
