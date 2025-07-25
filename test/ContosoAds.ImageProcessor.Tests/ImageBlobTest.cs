using System.Text.Json;

namespace ContosoAds.ImageProcessor.Tests;

public class ImageBlobTest
{
    [Fact]
    public void Can_Serialize_Json()
    {
        // Arrange
        var imageBlob = new ImageBlob
        {
            Uri = new Uri("https://fake.com/image.jpg"),
            AdId = 1
        };

        // Act
        var json = JsonSerializer.Serialize(imageBlob);

        // Assert
        Assert.Equal("""{"Uri":"https://fake.com/image.jpg","AdId":1}""", json);
    }
    
    [Fact]
    public void Can_Deserialize_Json()
    {
        // Arrange
        const string json = """{"Uri":"https://fake.com/image.jpg","AdId":1}""";

        // Act
        var imageBlob = JsonSerializer.Deserialize<ImageBlob>(json);

        // Assert
        Assert.Equal(new Uri("https://fake.com/image.jpg"), imageBlob?.Uri);
        Assert.Equal(1, imageBlob?.AdId);
    }
    
    [Theory]
    [MemberData(nameof(JsonStrings))]
    public void Rejects_Invalid_Json(string json)
    {
        // Assert/Act
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ImageBlob>(json));
    }
    
    public static TheoryData<string> JsonStrings =>
    [
        """{"Uri":"https://fake.com/image.jpg"}""",
        """{"AdId":1}""",
        """{"Uri":"","AdId":null}""",
        """{"Uri":null,"AdId":"1"}""",
        """{"Uri":"https://fake.com/image.jpg","Extra":"value"}"""
    ];
}