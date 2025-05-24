using System.Net;
using System.Net.Http.Json;

namespace ContosoAds.Web.Tests.Controllers;

public class ThumbnailControllerTest : IClassFixture<PostgreSqlContainerFixture>
{
    private readonly TestWebApplicationFactory _factory;
    
    public ThumbnailControllerTest(PostgreSqlContainerFixture fixture)
    {
        _factory = new TestWebApplicationFactory(fixture.ConnectionString);
    }

    [Fact]
    public async Task Options_Returns_Ok()
    {
        // Arrange
        using var client = _factory.CreateClient();
        
        // Act
        using var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Options, "/thumbnail-result"), 
            TestContext.Current.CancellationToken);
        
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
            new ImageBlob
            {
                Uri = new Uri(thumbnailUri),
                AdId = adId
            },
            TestContext.Current.CancellationToken);

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
            new ImageBlob
            {
                Uri = new Uri("https://contosoads.blob.core.windows.net/images/thumbnail.jpg"),
                AdId = 1
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(null, "1")]
    [InlineData("", "1")]
    [InlineData("relative_uri", "1")]
    [InlineData("https://example.com/image.jpg", null)]
    [InlineData("relative_uri", "not_an_int")]
    public async Task Post_WithInvalidContent_Returns_BadRequest(string? uri, string? adId)
    {
        // Arrange
        await _factory.SeedDatabaseAsync();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-result",
            new {uri, adId},
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}