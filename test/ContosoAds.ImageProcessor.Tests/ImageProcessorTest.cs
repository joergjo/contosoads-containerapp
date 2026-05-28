using Microsoft.Extensions.Logging.Testing;
using SkiaSharp;

namespace ContosoAds.ImageProcessor.Tests;

public class ImageProcessorTest
{
    [Theory]
    [MemberData(nameof(Images))]
    public async Task It_Renders_ThumbnailSize(string imageBase64, int expectedWidth, int expectedHeight)
    {
        // Arrange
        var logger = new FakeLogger<ImageProcessor>();
        var imageProcessor = new ImageProcessor(logger);
        await using var imageStream = new MemoryStream(Convert.FromBase64String(imageBase64));

        // Act
        await using var result = new MemoryStream();
        await imageProcessor.RenderAsync(imageStream, result);

        // Assert
        result.Position = 0;
        using var bitmap = SKBitmap.Decode(result);
        Assert.NotNull(bitmap);
        Assert.Equal(expectedWidth, bitmap.Width);
        Assert.Equal(expectedHeight, bitmap.Height);
    }

    public static TheoryData<string, int, int> Images =>
        new()
        {
            { Jpeg100By200, 40, 80 },
            { Jpeg200By100, 80, 40 },
            { Jpeg160By160, 80, 80 }
        };
    
}