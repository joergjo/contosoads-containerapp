using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using static ContosoAds.Web.UnitTests.TestSupport;

namespace ContosoAds.Web.UnitTests;

public class ListAdsTest
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
        const string dbName = nameof(ListAdsTest);
        var logger = A.Fake<ILogger<ListAds>>();
        var ads = new List<Ad>();
        for (var i = 0; i < numberOfAds; i++)
        {
            ads.Add(new Ad {Title = $"Some item #{i}", Price = 42, Phone = "425-555-1212"});
        }

        await using var initialDbContext = await CreateTestDbContext(dbName, true, ads);
        await using var testDbContext = await CreateTestDbContext(dbName);

        // Act
        var command = new ListAds(testDbContext, logger);
        var result = await command.ExecuteAsync(default);
       
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
        const string dbName = nameof(ListAdsTest);
        var logger = A.Fake<ILogger<ListAds>>();
        var ads = new[]
        {
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.FreeStuff, Phone = "425-555-1212", Price = 0, Title = "Junk" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car"  },
            new Ad { Category = Category.RealEstate, Phone = "425-555-1212", Price = 250000, Title = "Condo" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car"  },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car"  },
            new Ad { Category = Category.RealEstate, Phone = "425-555-1212", Price = 250000, Title = "Condo" },
            new Ad { Category = Category.FreeStuff, Phone = "425-555-1212", Price = 0, Title = "Junk" },
            new Ad { Category = Category.Cars, Phone = "425-555-1212", Price = 20000, Title = "Car" },
            new Ad { Category = Category.RealEstate, Phone = "425-555-1212", Price = 250000, Title = "Condo" }
        };

        await using var initialDbContext = await CreateTestDbContext(dbName, true, ads);
        await using var testDbContext = await CreateTestDbContext(dbName);

        // Act
        var command = new ListAds(testDbContext, logger);
        var result = await command.ExecuteAsync(category);

        Assert.Equal(expected, result.Count);
    }
}