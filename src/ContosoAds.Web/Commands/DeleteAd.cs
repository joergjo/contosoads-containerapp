using ContosoAds.Web.DataAccess;
using Dapr.Client;

namespace ContosoAds.Web.Commands;

public class DeleteAd(AdsContext dbContext, DaprClient daprClient, ILogger<DeleteAd> logger)
{
    public async Task<bool> ExecuteAsync(int id)
    {
        var ad = await dbContext.Ads.FindAsync(id);

        if (ad is null)
        {
            return false;
        }

        dbContext.Ads.Remove(ad);
        await dbContext.SaveChangesAsync();

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
        logger.LogDebug("Deleting image blob '{ImageUri}'", blobUri);
        var metadata = new Dictionary<string, string>
        {
            ["blobName"] = new Uri(blobUri).Segments[^1],
            ["deleteSnapshots"] = "include" 
        };
        await daprClient.InvokeBindingAsync<string?>("web-storage", "delete", null, metadata);
        logger.LogDebug("Deleted image blob '{ImageUri}'", blobUri);
    }
}