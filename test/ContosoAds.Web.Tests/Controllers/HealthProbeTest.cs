using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContosoAds.Web.Tests.Controllers;

public class HealthProbeTest : IClassFixture<PostgreSqlContainerFixture>
{
    private readonly TestWebApplicationFactory _factory;
    
    public HealthProbeTest(PostgreSqlContainerFixture fixture)
    {
        _factory = new TestWebApplicationFactory(fixture.ConnectionString);
    }
    
    [Theory]
    [InlineData("/healthz/live")]
    [InlineData("/healthz/ready")]
    public async Task Probe_Returns_OK(string uri)
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Assert
        using var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        // Act
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}