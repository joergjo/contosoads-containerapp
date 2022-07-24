using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace ContosoAds.ImageProcessor;

public class CloudRoleNameInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (!string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName)) return;

        telemetry.Context.Cloud.RoleName = "contosoads-imageprocessor";
    }
}