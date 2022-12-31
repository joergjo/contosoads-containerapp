using System.Text.Json.Serialization;

namespace ContosoAds.ImageProcessor;

// This is way more complex than it should be, but is required to work around limitations in System.Text.Json.
// See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-6-0#required-properties
// for more details.
// TODO: Required attributes can now be handled natively by using JsonRequiredAttribute. 
[JsonConverter(typeof(ImageBlobConverter))]
public record ImageBlob(Uri Uri, int AdId)
{
    [JsonIgnore] public string Name => Uri.Segments[^1];
}

public record JsonImageBlob(Uri Uri, int AdId) : ImageBlob(Uri, AdId);