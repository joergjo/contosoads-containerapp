using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ContosoAds.ImageProcessor;

public class ImageProcessor
{
    private readonly ILogger<ImageProcessor> _logger;

    public ImageProcessor(ILogger<ImageProcessor> logger)
    {
        _logger = logger;
    }

    public async Task RenderAsync(Stream input, Stream output)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (!input.CanRead)
        {
            throw new ArgumentException("Stream must be readable", nameof(input));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (!output.CanWrite)
        {
            throw new ArgumentException("Stream must be writable", nameof(output));
        }

        using var thumbnailImage = await ResizeImageAsync(input);
        await thumbnailImage.SaveAsJpegAsync(output);
    }

    private async Task<Image> ResizeImageAsync(Stream input, int size = 80)
    {
        var image = await Image.LoadAsync(input);
        var (width, height) = GetEffectiveSize(image.Width, image.Height, size);

        _logger.LogDebug(
            "Resized image from {OriginalWidth}x{OriginalHeight} to {NewWidth}x{NewHeight}",
            image.Width,
            image.Height,
            width,
            height);

        image.Mutate(ctx =>
        {
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(width, height)
            };
            ctx.Resize(options);
        });

        return image;
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