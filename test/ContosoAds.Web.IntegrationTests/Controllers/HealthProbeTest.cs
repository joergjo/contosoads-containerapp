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
    
    [Fact]
    public async Task Live_Returns_OK()
    {
        // Arrange
        using var client = _factory.CreateClient();
        
        // Act
        using var response = await client.GetAsync("/healthz/live");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Ready_Returns_OK()
    {
        // Arrange
        using var client = _factory.CreateClient();
        
        // Act
        using var response = await client.GetAsync("/healthz/ready");
    
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}