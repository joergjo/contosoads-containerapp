using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;
using Dapr.Client;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static ContosoAds.Web.UnitTests.TestSupport;

namespace ContosoAds.Web.UnitTests;

public class CreateOrEditTest
{
    [Theory]
    [InlineData("A car", "Nice ride", "+1 425 555 1234", 5000, Category.Cars)]
    [InlineData("A freebie", "Nice junk", "+1 425 555 1234", 0, Category.FreeStuff)]
    [InlineData("A house", "Nice real estate", "+1 425 555 1234", 1000000, Category.RealEstate)]
    public async Task It_Creates_Ad(
        string title,
        string description,
        string phone,
        int price,
        Category category)
    {
        // Arrange
        const string dbName = nameof(CreateOrEditTest);
        var logger = A.Fake<ILogger<CreateOrEditAd>>();
        var daprClient = A.Fake<DaprClient>();
        await using var initialDbContext = await CreateTestDbContext(dbName);
        var ad = new Ad
        {
            Title = title,
            Description = description,
            Phone = phone,
            Price = price,
            Category = category
        };

        // Act
        var command = new CreateOrEditAd(initialDbContext, daprClient, logger);
        await command.ExecuteAsync(ad, default);

        // Assert
        await using var testDbContext = await CreateTestDbContext(dbName);
        var createdAd = await testDbContext.Ads.FindAsync(ad.Id);
        Assert.Equal(title, createdAd?.Title);
        Assert.Equal(description, createdAd?.Description);
        Assert.Equal(phone, createdAd?.Phone);
        Assert.Equal(price, createdAd?.Price);
        Assert.Equal(category, createdAd?.Category);
    }

    [Theory]
    [InlineData("A car", "Nice ride", "+1 425 555 1234", 5000, Category.Cars, "car.jpg")]
    [InlineData("A freebie", "Nice junk", "+1 425 555 1234", 0, Category.FreeStuff, "freebie.jpg")]
    [InlineData("A house", "Nice real estate", "+1 425 555 1234", 1000000, Category.RealEstate, "house.jpg")]
    public async Task It_Sets_Image_For_Ad(
        string title,
        string description,
        string phone,
        int price,
        Category category,
        string fileName)
    {
        // Arrange
        const string dbName = nameof(CreateOrEditTest);
        var logger = A.Fake<ILogger<CreateOrEditAd>>();
        var daprClient = A.Fake<DaprClient>();
        A.CallTo(() => daprClient.InvokeBindingAsync<byte[], JsonNode>(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<IReadOnlyDictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.FromResult(CreateBlobResponse(fileName)));
        await using var initialDbContext = await CreateTestDbContext(dbName);

        var formFile = A.Fake<IFormFile>();
        A.CallTo(() => formFile.FileName).Returns(fileName);
        A.CallTo(() => formFile.Length).Returns(1);
        A.CallTo(() => formFile.ContentType).Returns("image/jpeg");
        A.CallTo(() => formFile.OpenReadStream()).Returns(new MemoryStream());

        var ad = new Ad
        {
            Title = title,
            Description = description,
            Phone = phone,
            Price = price,
            Category = category
        };

        // Act
        var command = new CreateOrEditAd(initialDbContext, daprClient, logger);
        await command.ExecuteAsync(ad, formFile);

        // Assert
        await using var testDbContext = await CreateTestDbContext(dbName);
        var createdAd = await testDbContext.Ads.FindAsync(ad.Id);
        Assert.NotNull(createdAd?.ImageUri);
        A.CallTo(() => daprClient.InvokeBindingAsync<byte[], JsonNode>(
                A<string>._, 
                A<string>._,
                A<byte[]>._,
                A<IReadOnlyDictionary<string, string>>._, 
                default))
            .MustHaveHappenedOnceExactly();
    }
}