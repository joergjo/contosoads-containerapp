using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.DataAccess;

public class AdsContext : DbContext
{
    public AdsContext(DbContextOptions<AdsContext> options) : base(options)
    {
    }

    public DbSet<Ad> Ads => Set<Ad>();
}