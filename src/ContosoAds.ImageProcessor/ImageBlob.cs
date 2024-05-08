using System.Text.Json.Serialization;

namespace ContosoAds.ImageProcessor;

public record ImageBlob
{
    public required Uri? Uri { get; init; }
    public required int AdId { get; init; }

    [JsonIgnore] 
    public string? Name => Uri is { IsAbsoluteUri: true } ? Uri.Segments[^1] : string.Empty;

    [JsonIgnore] 
    public bool IsValid => Uri is { IsAbsoluteUri: true } && AdId > 0;
}
