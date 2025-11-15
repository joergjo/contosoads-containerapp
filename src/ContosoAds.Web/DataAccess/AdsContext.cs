using ContosoAds.Web.Model;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.DataAccess;

public class AdsContext(DbContextOptions<AdsContext> options) : DbContext(options)
{
    public DbSet<Ad> Ads => Set<Ad>();
}