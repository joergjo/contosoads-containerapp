using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.Commands;

public class ListAds
{
    private readonly AdsContext _dbContext;
    private readonly ILogger<ListAds> _logger;

    public ListAds(AdsContext dbContext, ILogger<ListAds> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IList<Ad>> ExecuteAsync(Category? category)
    {
        var query = from ad in _dbContext.Ads
            select ad;
        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }

        var ads = await query.AsNoTracking().ToListAsync();
        _logger.LogDebug("Ads found: {Count}", ads.Count);
        return ads;
    }
}