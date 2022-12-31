using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ContosoAds.Web;

public static class WebApplicationExtensions
{
    public static WebApplication UseHealthChecks(this WebApplication app, int port, params string[] tags)
    {
        if (port > 0)
        {
            var host = $"*:{port}";
            app.MapHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = _ => false
            }).RequireHost(host);
            app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = r => tags.Contains(r.Name)
            }).RequireHost(host);
        }
        else
        {
            app.MapHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = r => tags.Contains(r.Name)
            });
        }

        return app;
    }
}