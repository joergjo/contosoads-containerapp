using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Dapr.Client;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContosoAds.Web.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _root = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AdsContext>>();
            services.AddDbContext<AdsContext>(options => 
                options.UseInMemoryDatabase("ContosoAds", _root));
            services.RemoveAll<DaprClient>();
            services.AddSingleton(A.Fake<DaprClient>());
        });
    }

    public async Task<IEnumerable<int>> SeedDatabaseAsync(params Ad[] ads)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AdsContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        if (ads.Length > 0)
        {
            await context.Ads.AddRangeAsync(ads);
            await context.SaveChangesAsync();
        }

        return ads.Select(a => a.Id);
    }
}