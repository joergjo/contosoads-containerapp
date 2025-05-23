namespace ContosoAds.Web.Tests.Commands;

public class ReadAdTest(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task It_Finds_Ad()
    {
        // Arrange
        const int adId = 1;
        var dbContext = await CreateAdsContext(
            fixture.ConnectionString,
            true,
            new Ad {Id = adId, Title = "Test Ad", Phone = "425-555-1212", Price = 42});

        // Act
        var command = new ReadAd(dbContext);
        var foundAd = await command.ExecuteAsync(adId);

        // Assert
        Assert.Equal(adId, foundAd?.Id);
    }

    [Fact]
    public async Task It_Returns_Null_If_Ad_Does_Not_Exist()
    {
        // Arrange
        var dbContext = await CreateAdsContext(
            fixture.ConnectionString, 
            true, 
            new Ad {Id = 1, Title = "Test Ad", Phone = "425-555-1212", Price = 42});

        // Act
        var command = new ReadAd(dbContext);
        var foundAd = await command.ExecuteAsync(2);

        // Assert
        Assert.Null(foundAd);
    }
}