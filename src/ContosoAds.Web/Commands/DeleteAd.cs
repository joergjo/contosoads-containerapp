using ContosoAds.Web.DataAccess;
using Dapr.Client;

namespace ContosoAds.Web.Commands;

public class DeleteAd
{
    private readonly AdsContext _dbContext;
    private readonly DaprClient _daprClient;
    private readonly ILogger<DeleteAd> _logger;

    public DeleteAd(AdsContext dbContext, DaprClient daprClient, ILogger<DeleteAd> logger)
    {
        _dbContext = dbContext;
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(int id)
    {
        var ad = await _dbContext.Ads.FindAsync(id);

        if (ad is null)
        {
            return false;
        }

        _dbContext.Ads.Remove(ad);
        await _dbContext.SaveChangesAsync();

        if (ad.ImageUri is { Length: > 0 })
        {
            await DeleteImageBlob(ad.ImageUri);
        }

        if (ad.ThumbnailUri is { Length: > 0 })
        {
            await DeleteImageBlob(ad.ThumbnailUri);
        }

        return true;
    }

    private async Task DeleteImageBlob(string blobUri)
    {
        _logger.LogDebug("Deleting image blob '{ImageUri}'", blobUri);
        var metadata = new Dictionary<string, string>
        {
            ["blobName"] = new Uri(blobUri).Segments[^1],
            ["deleteSnapshots"] = "include" 
        };
        await _daprClient.InvokeBindingAsync<string?>("image-store", "delete", null, metadata);
        _logger.LogDebug("Deleted image blob '{ImageUri}'", blobUri);
    }
}