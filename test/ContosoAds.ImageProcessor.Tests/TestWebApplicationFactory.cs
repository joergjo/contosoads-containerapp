using Dapr.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContosoAds.ImageProcessor.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public DaprClient DaprClient { get; } = A.Fake<DaprClient>();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DaprClient>();
            services.AddSingleton(DaprClient);
        });
    }
}