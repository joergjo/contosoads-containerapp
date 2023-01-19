using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.IntegrationTests.Pages.Ads;

public class EditTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public EditTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ClientOptions.AllowAutoRedirect = false;
    }

    [Fact]
    public async Task Post_RedirectsTo_Ads()
    {
        // Arrange
        await _factory.SeedDatabaseAsync(
            new Ad
            {
                Id = 1,
                Category = Category.Cars,
                Description = "A car",
                ImageUri = "https://contosoads.blob.core.windows.net/images/car.jpg",
                Phone = "425-555-1212",
                PostedDate = DateTime.UtcNow,
                Price = 10000,
                ThumbnailUri = "https://contosoads.blob.core.windows.net/images/thumbnail.jpg",
                Title = "A car"
            });
        using var client = _factory.CreateClient();
        using var getResponse = await client.GetAsync($"/ads/edit/1");
        using var document = await getResponse.ToDocumentAsync();
        var csrfToken = document.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/ads/edit/1")
        {
            Content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("Ad.Id", "1"),
                    new KeyValuePair<string, string>("Ad.Category", "Cars"),
                    new KeyValuePair<string, string>("Ad.Description", "Updated Description"),
                    new KeyValuePair<string, string>("Ad.Phone", "425-555-1212"),
                    new KeyValuePair<string, string>("Ad.Price", "20000"),
                    new KeyValuePair<string, string>("Ad.Title", "Test Ad"),
                    new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken!)
                })
        };
        using var postResponse = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/ads", postResponse.Headers.Location?.ToString().ToLower());
    }
    
    [Fact]
    public async Task Post_ForInValidId_Returns_NotFound()
    {
        // Arrange
        await _factory.SeedDatabaseAsync(
            new Ad
            {
                Id = 1,
                Category = Category.Cars,
                Description = "A car",
                ImageUri = "https://contosoads.blob.core.windows.net/images/car.jpg",
                Phone = "425-555-1212",
                PostedDate = DateTime.UtcNow,
                Price = 10000,
                ThumbnailUri = "https://contosoads.blob.core.windows.net/images/thumbnail.jpg",
                Title = "A car"
            });
        using var client = _factory.CreateClient();
        using var getResponse = await client.GetAsync($"/ads/edit/1");
        using var document = await getResponse.ToDocumentAsync();
        var csrfToken = document.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value");
        await _factory.SeedDatabaseAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/ads/edit/1")
        {
            Content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("Ad.Id", "1"),
                    new KeyValuePair<string, string>("Ad.Category", "Cars"),
                    new KeyValuePair<string, string>("Ad.Description", "Updated Description"),
                    new KeyValuePair<string, string>("Ad.Phone", "425-555-1212"),
                    new KeyValuePair<string, string>("Ad.Price", "20000"),
                    new KeyValuePair<string, string>("Ad.Title", "Test Ad"),
                    new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken!)
                })
        };
        using var postResponse = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, postResponse.StatusCode);
    }
}