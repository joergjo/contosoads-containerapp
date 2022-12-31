using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace ContosoAds.ImageProcessor.UnitTests;

public class ImageProcessorTest
{
    [Theory]
    [MemberData(nameof(Images))]
    public async Task It_Renders_ThumbnailSize(string imageBase64, int expectedWidth, int expectedHeight)
    {
        // Arrange
        var logger = A.Fake<ILogger<ImageProcessor>>();
        var imageProcessor = new ImageProcessor(logger);
        await using var imageStream = new MemoryStream(Convert.FromBase64String(imageBase64)); 
        
        // Act
        await using var result = new MemoryStream();
        await imageProcessor.RenderAsync(imageStream, result);
        
        // Assert
        result.Position = 0;
        using var image = await Image.LoadAsync(result);
        Assert.Equal(expectedWidth, image.Width);
        Assert.Equal(expectedHeight, image.Height);
    }

    public static IEnumerable<object[]> Images => new List<object[]>
    {
        new object[] {TestImages.Jpeg100By200, 40, 80},
        new object[] {TestImages.Jpeg200By100, 80, 40},
        new object[] {TestImages.Jpeg160By160, 80, 80}
    };
}