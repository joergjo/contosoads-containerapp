using Dapr.Client;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContosoAds.ImageProcessor.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public DaprClient DaprClient { get; } = A.Fake<DaprClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ApplicationInsights:ConnectionString",
            "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=http://127.0.0.1");
        builder.UseSetting("DisableTelemetry", "true");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DaprClient>();
            services.AddSingleton(DaprClient);
        });
    }
}