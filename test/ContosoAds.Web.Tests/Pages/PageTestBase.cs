namespace ContosoAds.Web.Tests.Pages;

public abstract class PageTestBase : IClassFixture<PostgreSqlContainerFixture>
{
    protected PageTestBase(PostgreSqlContainerFixture fixture)
    {
        var factory = new TestWebApplicationFactory(fixture.ConnectionString);
        factory.ClientOptions.AllowAutoRedirect = false;
        WebApplicationFactory = factory;
    }
    
    protected TestWebApplicationFactory WebApplicationFactory { get; }
}