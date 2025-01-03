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
        IReadOnlyList<Ad>? ads = null)
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

        if (ads is null or { Count: 0 })
        {
            return context;
        }
        
        await context.Ads.AddRangeAsync(ads);
        await context.SaveChangesAsync();
        return context;
    }

    internal static async Task<AdsContext> CreateTestDbContext(string databaseName, bool recreate = false,
        params Ad[] ads) =>
        await CreateTestDbContext(databaseName, recreate, ads as IReadOnlyList<Ad>);

    internal static JsonNode CreateBlobResponse(string fileName) =>
        JsonNode.Parse($"{{\"blobURL\":\"https://www.example.com/{fileName}\"}}")!;
}