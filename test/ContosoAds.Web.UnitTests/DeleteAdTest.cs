using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;
using Dapr.Client;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using static ContosoAds.Web.UnitTests.TestSupport;

namespace ContosoAds.Web.UnitTests;

public class DeleteAdTest
{
    [Fact]
    public async Task It_Deletes_Ad()
    {
        // Arrange
        const string dbName = nameof(DeleteAdTest);
        const int adId = 1;
        var logger = A.Fake<ILogger<DeleteAd>>();
        var daprClient = A.Fake<DaprClient>();
        A.CallTo(() => daprClient.InvokeBindingAsync(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<Dictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        await using var dbContext = await CreateTestDbContext(
            dbName,
            true,
            new Ad
            {
                Id = adId, Title = "Test Ad", Phone = "425-555-1212", Price = 42,
                ImageUri = "http://example.com/image.jpg"
            });

        // Act
        var command = new DeleteAd(dbContext, daprClient, logger);
        var isDeleted = await command.ExecuteAsync(1);

        // Assert
        Assert.True(isDeleted);
    }

    [Fact]
    public async Task It_Deletes_Image()
    {
        // Arrange
        const string dbName = nameof(DeleteAdTest);
        const int adId = 1;
        var logger = A.Fake<ILogger<DeleteAd>>();
        var daprClient = A.Fake<DaprClient>();
        A.CallTo(() => daprClient.InvokeBindingAsync(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<Dictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        await using var dbContext = await CreateTestDbContext(
            dbName,
            true,
            new Ad
            {
                Id = adId, Title = "Test Ad", Phone = "425-555-1212", Price = 42,
                ImageUri = "http://example.com/image.jpg"
            });

        // Act
        var command = new DeleteAd(dbContext, daprClient, logger);
        await command.ExecuteAsync(1);

        // Assert
        A.CallTo(() => daprClient.InvokeBindingAsync(
            A<string>.That.IsEqualTo("image-store"),
            A<string>.That.IsEqualTo("delete"),
            A<string>.That.IsNull(),
            A<Dictionary<string, string>>.That.Matches(m => m.ContainsKey("blobName")),
            default)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task It_Deletes_Thumbnail()
    {
        // Arrange
        const string dbName = nameof(DeleteAdTest);
        const int adId = 1;
        var logger = A.Fake<ILogger<DeleteAd>>();
        var daprClient = A.Fake<DaprClient>();
        A.CallTo(() => daprClient.InvokeBindingAsync(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<Dictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        await using var dbContext = await CreateTestDbContext(
            dbName,
            true,
            new Ad
            {
                Id = adId, Title = "Test Ad", Phone = "425-555-1212", Price = 42,
                ThumbnailUri = "http://example.com/tn-image.jpg"
            });

        // Act
        var command = new DeleteAd(dbContext, daprClient, logger);
        await command.ExecuteAsync(1);

        // Assert
        A.CallTo(() => daprClient.InvokeBindingAsync(
            A<string>.That.IsEqualTo("image-store"),
            A<string>.That.IsEqualTo("delete"),
            A<string>.That.IsNull(),
            A<Dictionary<string, string>>.That.Matches(m => m.ContainsKey("blobName")),
            default)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task It_Deletes_Image_And_Thumbnail()
    {
        // Arrange
        const string dbName = nameof(DeleteAdTest);
        const int adId = 1;
        var logger = A.Fake<ILogger<DeleteAd>>();
        var daprClient = A.Fake<DaprClient>();
        A.CallTo(() => daprClient.InvokeBindingAsync(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<Dictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        await using var dbContext = await CreateTestDbContext(
            dbName,
            true,
            new Ad
            {
                Id = adId, Title = "Test Ad", Phone = "425-555-1212", Price = 42,
                ImageUri = "http://example.com/image.jpg", ThumbnailUri = "http://example.com/tn-image.jpg"
            });

        // Act
        var command = new DeleteAd(dbContext, daprClient, logger);
        await command.ExecuteAsync(1);

        // Assert
        A.CallTo(() => daprClient.InvokeBindingAsync(
            A<string>.That.IsEqualTo("image-store"),
            A<string>.That.IsEqualTo("delete"),
            A<string>.That.IsNull(),
            A<Dictionary<string, string>>.That.Matches(m => m.ContainsKey("blobName")),
            default)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task It_Does_Nothing_If_Ad_Does_Not_Exist()
    {
        // Arrange
        const string dbName = nameof(DeleteAdTest);
        const int adId = 1;
        var logger = A.Fake<ILogger<DeleteAd>>();
        var daprClient = A.Fake<DaprClient>();
        await using var initialDbContext = await CreateTestDbContext(
            dbName,
            true,
            new Ad {Id = adId, Title = "Test Ad", Phone = "425-555-1212", Price = 42});
        var numberOfAdsBefore = initialDbContext.Ads.Count();

        // Act
        var command = new DeleteAd(initialDbContext, daprClient, logger);
        var isDeleted = await command.ExecuteAsync(42);

        // Assert
        await using var testDbContext = await CreateTestDbContext(dbName);
        var numberOfAdsAfter = testDbContext.Ads.Count();
        Assert.False(isDeleted);
        Assert.Equal(numberOfAdsBefore, numberOfAdsAfter);
    }
}