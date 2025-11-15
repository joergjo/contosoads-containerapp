using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.Commands;

public class ListAds(AdsContext dbContext, ILogger<ListAds> logger)
{
    public async Task<IList<Ad>> ExecuteAsync(Category? category)
    {
        IQueryable<Ad> query = from ad in dbContext.Ads
            orderby ad.PostedDate
            select ad;
        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }

        var ads = await query.AsNoTracking().ToListAsync();
        logger.LogDebug("Ads found: {Count}", ads.Count);
        return ads;
    }
}