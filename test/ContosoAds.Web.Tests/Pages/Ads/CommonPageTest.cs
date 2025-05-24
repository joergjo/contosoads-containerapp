using System.Net;

namespace ContosoAds.Web.Tests.Pages.Ads;

public class CommonPageTest(PostgreSqlContainerFixture fixture) : PageTestBase(fixture)
{
    [Theory]
    [InlineData("delete", "Delete Ad")]
    [InlineData("details", "Details")]
    [InlineData("edit", "Edit Ad")]
    public async Task Get_Returns_OkAndPage(string action, string pageTitle)
    {
        // Arrange
        var id = (await WebApplicationFactory.SeedDatabaseAsync(
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
        using var client = WebApplicationFactory.CreateClient();

        // Act
        using var response = await client.GetAsync($"/ads/{action}/{id}", TestContext.Current.CancellationToken);

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
        await WebApplicationFactory.SeedDatabaseAsync();
        using var client = WebApplicationFactory.CreateClient();
        
        // Act
        using var response = await client.GetAsync($"/ads/{action}/{id}", TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}