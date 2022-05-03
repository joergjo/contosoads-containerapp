using ContosoAds.Web.Model;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContosoAds.Web.DataAccess;

public class AdsContext : DbContext, IDataProtectionKeyContext
{
    public AdsContext(DbContextOptions<AdsContext> options) : base(options)
    {
    }

    public DbSet<Ad> Ads => Set<Ad>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>(); 
}