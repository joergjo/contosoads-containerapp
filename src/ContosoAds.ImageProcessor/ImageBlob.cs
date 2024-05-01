using System.Text.Json.Serialization;

namespace ContosoAds.ImageProcessor;

public record ImageBlob
{
    public required Uri Uri { get; init; }
    public required int AdId { get; init; }

    [JsonIgnore] public string Name => Uri.Segments[^1];
}
