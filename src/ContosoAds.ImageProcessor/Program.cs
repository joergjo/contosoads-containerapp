using ContosoAds.ImageProcessor;
using Dapr;
using Dapr.Client;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>();
builder.Services.AddDaprClient();
builder.Services.AddScoped<ImageProcessor>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthz/live");
app.MapMethods("/thumbnail-request", new[] {HttpMethods.Options}, () => Results.Ok());
app.MapPost("/thumbnail-request",
    async (ImageBlob imageBlob, DaprClient client, ImageProcessor imageProcessor) =>
    {
        app.Logger.LogInformation("Received thumbnail image rendering request for '{BlobUri}'", imageBlob.Uri);

        byte[] rawData;
        try
        {
            rawData = await client.ReadAzureBlobAsync("image-store", imageBlob.Name);
        }
        catch (DaprException ex)
        {
            app.Logger.LogError(ex, "Failed to read image blob '{BlobUri}' from blob storage", imageBlob.Uri);
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
            thumbnailUri =
                await client.WriteAzureBlobBase64Async("image-store", $"tn-{imageBlob.Name}", output.ToArray(),
                    "image/jpeg");
        }
        catch (DaprException ex)
        {
            app.Logger.LogError(ex, "Failed to upload thumbnail for ad '{AdId} to image store", imageBlob.AdId);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Blob storage error",
                detail: $"Failed to upload thumbnail for ad '{imageBlob.AdId}' to image store");
        }

        app.Logger.LogInformation("Thumbnail for image with id '{AdId}' stored at '{ThumbnailUri}'", imageBlob.AdId,
            thumbnailUri);
        var thumbnailBlob = new ImageBlob(new Uri(thumbnailUri!), imageBlob.AdId);
        try
        {
            await client.InvokeBindingAsync("thumbnail-result", "create", thumbnailBlob);
        }
        catch (DaprException ex)
        {
            app.Logger.LogError(ex, "Failed to submit message for ad '{AdId} to result queue", imageBlob.AdId);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Messaging error",
                detail: $"Failed to submit result for ad '{imageBlob.AdId}' to result queue");
        }

        return Results.Ok();
    });

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program
{
}