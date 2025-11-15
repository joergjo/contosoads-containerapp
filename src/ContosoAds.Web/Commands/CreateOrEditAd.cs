using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.Commands;

public class CreateOrEditAd(AdsContext dbContext, DaprClient daprClient, ILogger<CreateOrEditAd> logger)
{
    private record struct CreateOrEditResult(bool HasUpdatedDb, bool HasNewImage = false);

    public async Task<bool> ExecuteAsync(Ad ad, IFormFile? file)
    {
        var (hasUpdatedDb, hasNewImage) = ad.IsNew ? await Create(ad, file) : await Edit(ad, file);
        if (!hasUpdatedDb)
        {
            return false;
        }

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "An error occurred while saving ad '{AdID}'", ad.Id);
            throw;
        }

        if (hasNewImage)
        {
            await RequestRenderThumbnail(ad);
        }

        return true;
    }

    private async Task<CreateOrEditResult> Create(Ad ad, IFormFile? file)
    {
        var hasNewImage = false;
        if (file is not null)
        {
            ad.ImageUri = await WriteImageBlob(file);
            hasNewImage = true;
            logger.LogDebug("Created image blob with URI '{ImageUri}' for new ad", ad.ImageUri);
        }
        else
        {
            logger.LogDebug("No image file provided for new ad");
        }

        dbContext.Ads.Add(ad);
        return new CreateOrEditResult(true, hasNewImage);
    }

    private async Task<CreateOrEditResult> Edit(Ad ad, IFormFile? file)
    {
        var existingAd = await dbContext.Ads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ad.Id);
        if (existingAd is null)
        {
            return new CreateOrEditResult(false);
        }

        var hasNewImage = false;
        if (file is not null)
        {
            // Note that we opt to create a new blob here to avoid caching issues and reset the thumbnail.
            ad.ImageUri = await WriteImageBlob(file);
            ad.ThumbnailUri = null;
            hasNewImage = true;
            logger.LogDebug("Created image blob with URI '{ImageUri}' for ad {AdId}", ad.ImageUri, ad.Id);
        }
        else
        {
            ad.ImageUri = existingAd.ImageUri;
            ad.ThumbnailUri = existingAd.ThumbnailUri;
            logger.LogDebug("No image file provided for ad {AdId}", ad.Id);
        }

        ad.PostedDate = existingAd.PostedDate;
        dbContext.Ads.Update(ad);
        return new CreateOrEditResult(true, hasNewImage);
    }

    private async Task<string?> WriteImageBlob(IFormFile file, string? currentUri = null)
    {
        await using var buffer = new MemoryStream(0x10000);
        await file.CopyToAsync(buffer);
        var newUri = await daprClient.WriteAzureBlobBase64Async("web-storage", GetBlobName(file.FileName, currentUri),
            buffer.ToArray(), file.ContentType);

        if (newUri is null)
        {
            logger.LogWarning("Failed to upload '{FileName}' to image store", file.FileName);
        }

        return newUri;
    }

    private async Task RequestRenderThumbnail(Ad ad)
    {
        var imageBlob = new ImageBlob
        {
            Uri = new Uri(ad.ImageUri!),
            AdId = ad.Id
        };
        await daprClient.InvokeBindingAsync("thumbnail-request-sender", "create", imageBlob);
        logger.LogDebug("Requested thumbnail rendering for ad {AdId}", ad.Id);
    }

    private static string GetBlobName(string fileName, string? blobUri)
    {
        if (blobUri is { Length: > 0 })
        {
            var uri = new Uri(blobUri);
            return uri.Segments[^1];
        }

        // We're only interested in the original file name's extension
        return $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
    }
}