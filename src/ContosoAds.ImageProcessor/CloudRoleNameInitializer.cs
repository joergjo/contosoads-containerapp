using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace ContosoAds.ImageProcessor;

public class CloudRoleNameInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry.Context.Cloud.RoleName is { Length: > 0 }) return;

        telemetry.Context.Cloud.RoleName = "contosoads-imageprocessor";
    }
}