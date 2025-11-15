using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ContosoAds.Web;

public static class WebApplicationExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    extension(WebApplication app)
    {
        public WebApplication UseHealthChecks(int port, params string[] tags)
        {
            string[] hosts = port > 0 ? [$"*:{port}"] : [];
            var options = new HealthCheckOptions { Predicate = r => tags.Contains(r.Name) };

            app.MapHealthChecks("/healthz/live", new HealthCheckOptions { Predicate = _ => false })
                .RequireHost(hosts);
            app.MapHealthChecks("/healthz/ready", options)
                .RequireHost(hosts);

            return app;
        }
    }
}