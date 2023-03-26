using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ContosoAds.Web.IntegrationTests.Controllers;

public class HealthProbeTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    
    public HealthProbeTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    [Theory]
    [InlineData("/healthz/live")]
    [InlineData("/healthz/ready")]
    public async Task Probe_Returns_OK(string uri)
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Assert
        using var response = await client.GetAsync(uri);

        // Act
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}