using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.UnitTests;

internal static class TestSupport
{
    internal static async Task<AdsContext> CreateTestDbContext(string databaseName, bool recreate = false,
        IReadOnlyList<Ad>? ads = default)
    {
        var options = new DbContextOptionsBuilder<AdsContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;
        var context = new AdsContext(options);
        
        if (recreate)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        // ReSharper disable once InvertIf
        if (ads is {Count: > 0})
        {
            await context.Ads.AddRangeAsync(ads);
            await context.SaveChangesAsync();
        }

        return context;
    }

    internal static async Task<AdsContext> CreateTestDbContext(string databaseName, bool recreate = false,
        params Ad[] ads) =>
        await CreateTestDbContext(databaseName, recreate, ads as IReadOnlyList<Ad>);

    internal static JsonNode CreateBlobResponse(string fileName) =>
        JsonNode.Parse($"{{\"blobURL\":\"https://www.example.com/{fileName}\"}}")!;
}