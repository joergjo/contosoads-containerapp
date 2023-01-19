using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.IntegrationTests.Pages.Ads;

public class CommonPageTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CommonPageTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ClientOptions.AllowAutoRedirect = false;
    }
 
    [Theory]
    [InlineData("delete", "Delete Ad")]
    [InlineData("details", "Details")]
    [InlineData("edit", "Edit Ad")]
    public async Task Get_Returns_OkAndPage(string action, string pageTitle)
    {
        // Arrange
        var id = (await _factory.SeedDatabaseAsync(
            new Ad
            {
                Category = Category.Cars,
                Description = "A car",
                ImageUri = "https://contosoads.blob.core.windows.net/images/car.jpg",
                Phone = "425-555-1212",
                PostedDate = DateTime.UtcNow,
                Price = 10000,
                ThumbnailUri = "https://contosoads.blob.core.windows.net/images/thumbnail.jpg",
                Title = "A car"
            })).First();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync($"/ads/{action}/{id}");

        // Assert
        using var document = await response.ToDocumentAsync();
        var title = document.QuerySelector("title")?.TextContent;
        Assert.NotNull(title);
        Assert.StartsWith(pageTitle, title);
    }
    
    [Theory]
    [InlineData("9999", "details")]
    [InlineData("abc", "details")]
    [InlineData("this cannot work", "details")]
    [InlineData("9999", "delete")]
    [InlineData("abc", "delete")]
    [InlineData("this cannot work", "delete")]
    [InlineData("9999", "edit")]
    [InlineData("abc", "edit")]
    [InlineData("this cannot work", "edit")]
    public async Task Get_WithInvalidId_Returns_NotFound(string id, string action)
    {
        // Arrange
        await _factory.SeedDatabaseAsync();
        using var client = _factory.CreateClient();
        
        // Act
        using var response = await client.GetAsync($"/ads/{action}/{id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}