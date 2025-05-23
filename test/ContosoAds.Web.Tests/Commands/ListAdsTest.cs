namespace ContosoAds.Web.Tests.Commands;

public class ListAdsTest(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(66)]
    [InlineData(100)]
    public async Task It_Finds_Ads(int numberOfAds)
    {
        // Arrange
        var logger = new FakeLogger<ListAds>();
        var ads = new List<Ad>();
        for (var i = 0; i < numberOfAds; i++)
        {
            ads.Add(new Ad { Title = $"Some item #{i}", Price = 42, Phone = "425-555-1212" });
        }

        await using var initialDbContext = await CreateAdsContext(
            fixture.ConnectionString, 
            true, 
            ads);
        await using var testDbContext = await CreateAdsContext(fixture.ConnectionString);

        // Act
        var command = new ListAds(testDbContext, logger);
        var result = await command.ExecuteAsync(null);

        Assert.Equal(numberOfAds, result.Count);
    }

    [Theory]
    [InlineData(Category.Cars, 5)]
    [InlineData(Category.RealEstate, 3)]
    [InlineData(Category.FreeStuff, 2)]
    [InlineData(null, 10)]
    public async Task It_Find_Ads_By_Category(Category? category, int expected)
    {
        // Arrange
        var logger = new FakeLogger<ListAds>();
        Ad[] ads =
        [
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.FreeStuff, Phone = "425-555-1212", Price = 0, Title = "Junk" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.RealEstate, Phone = "425-555-1212", Price = 250000, Title = "Condo" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.RealEstate, Phone = "425-555-1212", Price = 250000, Title = "Condo" },
            new Ad { Category = Category.FreeStuff, Phone = "425-555-1212", Price = 0, Title = "Junk" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.RealEstate, Phone = "425-555-1212", Price = 250000, Title = "Condo" }
        ];

        await using var initialDbContext = await CreateAdsContext(
            fixture.ConnectionString, 
            true, 
            ads);
        await using var testDbContext = await CreateAdsContext(fixture.ConnectionString);

        // Act
        var command = new ListAds(testDbContext, logger);
        var result = await command.ExecuteAsync(category);

        Assert.Equal(expected, result.Count);
    }
}