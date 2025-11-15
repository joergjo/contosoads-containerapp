using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.Commands;

public class ReadAd(AdsContext dbContext)
{
    public async Task<Ad?> ExecuteAsync(int id) => await dbContext.Ads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
}