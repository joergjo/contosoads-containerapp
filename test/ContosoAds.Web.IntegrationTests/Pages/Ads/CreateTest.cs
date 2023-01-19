using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContosoAds.Web.IntegrationTests.Pages.Ads;

public class CreateTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CreateTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ClientOptions.AllowAutoRedirect = false;
    }

    [Theory]
    [InlineData("/ads/create")]
    [InlineData("/ads/create/")]
    public async Task Get_Returns_OkAndPage(string uri)
    {
        // Arrange
        await _factory.SeedDatabaseAsync();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync(uri);

        // Assert
        using var document = await response.ToDocumentAsync();
        var title = document.QuerySelector("title")?.TextContent;
        Assert.NotNull(title);
        Assert.StartsWith("Create Ad", title);
    }

    [Theory]
    [InlineData("/ads/create/9999")]
    [InlineData("/ads/create/abc")]
    [InlineData("/ads/create/more/than/one/part")]
    [InlineData("/ads/create/ad/1")]
    public async Task Get_WithAdditionalSegments_Returns_NotFound(string uri)
    {
        // Arrange
        await _factory.SeedDatabaseAsync();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task Post_RedirectsTo_Ads()
    {
        // Arrange
        const string uri = "/ads/create";
        await _factory.SeedDatabaseAsync();
        var client = _factory.CreateClient();
        using var getResponse = await client.GetAsync(uri);
        using var document = await getResponse.ToDocumentAsync();
        var csrfToken = document.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value");
        
        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("Ad.Category", "Cars"),
                    new KeyValuePair<string, string>("Ad.Description", "Test Description"),
                    new KeyValuePair<string, string>("Ad.Phone", "425-555-1212"),
                    new KeyValuePair<string, string>("Ad.Price", "10000"),
                    new KeyValuePair<string, string>("Ad.Title", "Test Ad"),
                    new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken!)
                })
        };
        using var postResponse = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/ads", postResponse.Headers.Location?.ToString().ToLower());
    }
}