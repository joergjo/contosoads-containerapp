using Oakton;

namespace ContosoAds.Cli;

public class CreateAdInput
{
    [Description("URL of ContosoAds web application")]
    public Uri Uri { get; set; } = new Uri("https://localhost:8080");
    
    [Description("Number of ads to create")]
    public int Count { get; set; } = 1;
    
    [Description("Timeout per ad creation in seconds")]
    public int Timeout { get; set; } = 5;
    
    [Description("Path to the image file")]
    public string ImageFile { get; set; } = "car.jpg";
    
    [Description("MIME type of the image file")]
    public string MimeType { get; set; } = "image/jpeg";
}