using System.Text.Json.Nodes;

namespace ContosoAds.ImageProcessor.Tests;

internal static class TestSupport
{
    internal static JsonNode CreateBlobResponse(string fileName) =>
        JsonNode.Parse($"{{\"blobURL\":\"https://www.example.com/{fileName}\"}}")!;
}