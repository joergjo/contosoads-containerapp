namespace ContosoAds.Web.Model;

public record ImageBlob
{
    public required Uri Uri { get; init; }
    public required int AdId { get; init; }
}