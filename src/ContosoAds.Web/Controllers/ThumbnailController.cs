using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.AspNetCore.Mvc;

namespace ContosoAds.Web.Controllers;

[ApiController]
[Route("/thumbnail-result")]
public class ThumbnailController : Controller
{
    private readonly AdsContext _dbContext;
    private readonly ILogger<ThumbnailController> _logger;

    public ThumbnailController(AdsContext dbContext, ILogger<ThumbnailController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpOptions]
    public IActionResult Options() => Ok();

    [HttpPost]
    public async Task<IActionResult> HandleThumbnailCreated(
        ImageBlob imageBlob,
        CancellationToken cancellationToken = default)
    {
        var (uri, adId) = (imageBlob.Uri, imageBlob.AdId);
        // The built-in model binder allows both absolute and relative URIs. We expect an absolute URI.
        // This should not happen unless someone POSTs junk requests directly to our endpoint.
        if (!uri.IsAbsoluteUri)
        {
            _logger.LogError(
                "Failed to set thumbnail URL '{ThumbnailUri}' for ad '{AdId}' because it is not an absolute URI",
                uri, adId);
            return BadRequest();
        }

        var ad = await _dbContext.Ads.FindAsync(new object[] {adId}, cancellationToken);
        if (ad is null)
        {
            _logger.LogWarning(
                "Failed to set thumbnail URL '{ThumbnailUri}' for ad '{AdId}' because it was not found in database",
                uri, adId);
            // This is a valid scenario - a user might have deleted the ad already before the thumbnail was created.
            return Ok();
        }

        ad.ThumbnailUri = uri.ToString();
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Set thumbnail URL '{ThumbnailUri}' for ad '{AdId}'", uri, adId);
        return Ok();
    }
}