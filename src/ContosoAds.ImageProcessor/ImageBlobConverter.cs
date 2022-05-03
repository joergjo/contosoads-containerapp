using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContosoAds.ImageProcessor;

public class ImageBlobConverter : JsonConverter<ImageBlob>
{
    public override ImageBlob Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var imageBlob = JsonSerializer.Deserialize<JsonImageBlob>(ref reader, options)!;
        if (imageBlob.AdId <= 0 || imageBlob.Uri is null)
        {
            throw new JsonException("Invalid image blob. Either the ad ID or the URI property or both are invalid.");
        }

        return imageBlob;
    }

    public override void Write(Utf8JsonWriter writer, ImageBlob imageBlob, JsonSerializerOptions options)
    {
        var imageBlobJson = new JsonImageBlob(imageBlob.Uri, imageBlob.AdId);
        JsonSerializer.Serialize(writer, imageBlobJson, options);
    }
}