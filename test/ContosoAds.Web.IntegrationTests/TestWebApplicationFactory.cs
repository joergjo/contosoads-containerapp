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

    public DaprClient DaprClient { get; } = A.Fake<DaprClient>();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AdsContext>>();
            services.AddDbContext<AdsContext>(options =>
            {
                // In .NET 9, there must have been a change in the way the
                // services are registered, which causes tests to fail
                // because Entity Framework still resolves the Npgsql
                // provider. To fix this, we use an internal service provider
                // to resolve the Entity Framework services.
                var efServices = new ServiceCollection();
                efServices.AddEntityFrameworkInMemoryDatabase();
                var provider = efServices.BuildServiceProvider();
                options.UseInternalServiceProvider(provider);
                options.UseInMemoryDatabase("ContosoAds", _root);
            });
            services.RemoveAll<DaprClient>();
            services.AddSingleton(DaprClient);
        });
    }

    public async Task<IEnumerable<int>> SeedDatabaseAsync(params Ad[] ads)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AdsContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        if (ads.Length == 0)
        {
            return [];
        }
        
        await context.Ads.AddRangeAsync(ads);
        await context.SaveChangesAsync();
        return ads.Select(a => a.Id);
    }
}