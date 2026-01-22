using Azure.Monitor.OpenTelemetry.AspNetCore;
using ContosoAds.ImageProcessor;
using Dapr;
using Dapr.Client;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

var appInsightsConnectionString = builder.Configuration.GetValue<string?>(
    "APPLICATIONINSIGHTS_CONNECTION_STRING");
if (appInsightsConnectionString is { Length: > 0 })
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(configure => configure.EnableLiveMetrics = true);
    builder.Services.ConfigureOpenTelemetryMeterProvider((_, configure) => configure.AddRuntimeInstrumentation());
    builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
    {
        options.RecordException = true;
        options.Filter = context => !context.Request.Path.StartsWithSegments("/healthz");
    });

}

builder.Services.AddDaprClient();
builder.Services.AddScoped<ImageProcessor>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthz/live");
app.MapHealthChecks("/healthz/ready");
app.MapMethods("/thumbnail-request", [HttpMethods.Options], () => Results.Ok());
app.MapPost("/thumbnail-request",
    async (ImageBlob imageBlob, DaprClient client, ImageProcessor imageProcessor) =>
    {
        app.Logger.LogInformation("Received thumbnail image rendering request for '{BlobUri}'", imageBlob.Uri);
        if (!imageBlob.IsValid)
        {
            app.Logger.LogWarning("Received invalid image blob with URI '{BlobUri}' and AdId '{AdId}'", 
                imageBlob.Uri,
                imageBlob.AdId);
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Validation error",
                detail: $"Received invalid image blob with URI '{imageBlob.Uri}' and AdId '{imageBlob.AdId}");
        }
        
        byte[] rawData;
        try
        {
            rawData = await client.ReadAzureBlobAsync("imageprocessor-storage", imageBlob.Name!);
        }
        catch (DaprException ex)
        {
            app.Logger.LogError(ex, 
                "Failed to read image blob '{BlobUri}' from blob storage", 
                imageBlob.Uri);
            // The blob no longer exists in blob storage, hence we return OK. 
            return Results.Ok();
        }

        await using var input = new MemoryStream(rawData);
        // TODO: Add a setting for maximum body size in Dapr
        await using var output = new MemoryStream(rawData.Length / 4);
        await imageProcessor.RenderAsync(input, output);

        string? thumbnailUri;
        try
        {
            thumbnailUri = await client.WriteAzureBlobBase64Async("imageprocessor-storage", 
                $"tn-{imageBlob.Name}", 
                output.ToArray(),
                "image/jpeg");
        }
        catch (DaprException ex)
        {
            app.Logger.LogError(ex, 
                "Failed to upload thumbnail for ad '{AdId} to image store", 
                imageBlob.AdId);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Blob storage error",
                detail: $"Failed to upload thumbnail for ad '{imageBlob.AdId}' to image store");
        }

        app.Logger.LogInformation("Thumbnail for image with id '{AdId}' stored at '{ThumbnailUri}'", 
            imageBlob.AdId,
            thumbnailUri);
        var thumbnailBlob = imageBlob with { Uri = new Uri(thumbnailUri!) };
        try
        {
            await client.InvokeBindingAsync("thumbnail-result-sender", "create", thumbnailBlob);
        }
        catch (DaprException ex)
        {
            app.Logger.LogError(ex, 
                "Failed to submit message for ad '{AdId} to result queue", 
                imageBlob.AdId);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Messaging error",
                detail: $"Failed to submit result for ad '{imageBlob.AdId}' to result queue");
        }

        return Results.Ok();
    });

app.Run();
