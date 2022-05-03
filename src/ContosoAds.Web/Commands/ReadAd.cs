using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.Commands;

public class ReadAd
{
    private readonly AdsContext _dbContext;
    
    public ReadAd(AdsContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Ad?> ExecuteAsync(int id) => await _dbContext.Ads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
}