using System.Threading.Tasks;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.Tests.Pages.Ads;

public class IndexTest(PostgreSqlContainerFixture fixture) : PageTestBase(fixture)
{
    [Theory]
    [InlineData("/ads")]
    [InlineData("/ads/")]
    [InlineData("/ads?Category=x")]
    [InlineData("/ads/delete")]
    [InlineData("/ads/delete/")]
    [InlineData("/ads/edit")]
    [InlineData("/ads/edit/")]
    [InlineData("/ads/details")]
    [InlineData("/ads/details/")]    
    public async Task Get_Returns_OkAndPage(string uri)
    {
        // Arrange
        await WebApplicationFactory.SeedDatabaseAsync();
        using var client = WebApplicationFactory.CreateClient();

        // Act
        using var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        // Assert
        using var document = await response.ToDocumentAsync();
        var title = document.QuerySelector("title")?.TextContent;
        Assert.NotNull(title);
        Assert.StartsWith("Ad Listing", title);
    }
    
    [Theory]
    [InlineData(Category.Cars, "Cars")]
    [InlineData(Category.FreeStuff, "Free Stuff")]
    [InlineData(Category.RealEstate, "Real Estate")]
    public async Task Get_WithCategory_Returns_OkAndPage(Category category, string displayName)
   
    {
        // Arrange
        await WebApplicationFactory.SeedDatabaseAsync();
        using var client = WebApplicationFactory.CreateClient();

        // Act
        using var response = await client.GetAsync($"/ads?Category={category}", TestContext.Current.CancellationToken);

        // Assert
        using var document = await response.ToDocumentAsync();
        var rows = document.QuerySelectorAll("tbody tr");
        foreach (var row in rows)
        {
            Assert.Contains(displayName, row.InnerHtml);
        }
    }  
}