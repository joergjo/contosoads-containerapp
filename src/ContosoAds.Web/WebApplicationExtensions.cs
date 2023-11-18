using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ContosoAds.Web;

public static class WebApplicationExtensions
{
    public static WebApplication UseHealthChecks(this WebApplication app, int port, params string[] tags)
    {
        var host = port > 0 ? $"*:{port}" : null;
        var options = new HealthCheckOptions { Predicate = r => tags.Contains(r.Name) };

        app.MapHealthChecks("/healthz/live", new HealthCheckOptions { Predicate = _ => false })
           .RequireHost(host!);
        app.MapHealthChecks("/healthz/ready", options)
           .RequireHost(host!);

        return app;
    }
}