using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContosoAds.Web.Model;
using Xunit;

namespace ContosoAds.Web.IntegrationTests.Controllers;

public class ThumbnailControllerTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ThumbnailControllerTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Options_Returns_Ok()
    {
        // Arrange
        using var client = _factory.CreateClient();
        
        // Act
        using var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Options, "/thumbnail-result"));
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithExistingAd_Returns_Ok()
    {
        // Arrange
        const string thumbnailUri = "https://contosoads.blob.core.windows.net/images/thumbnail.jpg";
        const int adId = 1;

        await _factory.SeedDatabaseAsync(
            new Ad
            {
                Id = adId,
                Category = Category.Cars,
                Description = "A car",
                ImageUri = "https://contosoads.blob.core.windows.net/images/car.jpg",
                Phone = "425-555-1212",
                PostedDate = DateTime.UtcNow,
                Price = 10000,
                Title = "A car"
            });
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-result",
            new ImageBlob(new Uri(thumbnailUri), adId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingAd_Returns_Ok()
    {
        // Arrange
        await _factory.SeedDatabaseAsync();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-result",
            new ImageBlob(new Uri("https://contosoads.blob.core.windows.net/images/thumbnail.jpg"), 1));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(default, "1")]
    [InlineData("", "1")]
    [InlineData("relative_uri", "1")]
    [InlineData("https://example.com/image.jpg", default)]
    [InlineData("relative_uri", "not_an_int")]
    public async Task Post_WithInvalidContent_Returns_BadRequest(string? uri, string? adId)
    {
        // Arrange
        await _factory.SeedDatabaseAsync();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-result",
            new {uri, adId});

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}