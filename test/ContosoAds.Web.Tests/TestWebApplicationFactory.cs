using ContosoAds.Web.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContosoAds.Web.Tests;

public class TestWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    public DaprClient DaprClient { get; } = A.Fake<DaprClient>();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AdsContext>>();
            services.AddDbContext<AdsContext>(options => { options.UseNpgsql(connectionString); });
            services.RemoveAll<DaprClient>();
            services.AddSingleton(DaprClient);
        });
    }

    public async Task<IEnumerable<int>> SeedDatabaseAsync(params IReadOnlyList<Ad> ads)
    {
        using var scope = Services.CreateScope();
        return await TestSupport.SeedDatabaseAsync(scope, ads);
    }
}