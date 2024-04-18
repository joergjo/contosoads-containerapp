using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace ContosoAds.Web;

public class TelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry.Context.Cloud.RoleName is { Length: > 0 }) return;

        telemetry.Context.Cloud.RoleName = "contosoads-web";
    }
}