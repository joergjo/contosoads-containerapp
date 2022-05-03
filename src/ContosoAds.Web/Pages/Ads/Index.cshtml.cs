using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoAds.Web.Commands;
using ContosoAds.Web.DataAccess;
using ContosoAds.Web.Model;
using Microsoft.AspNetCore.Mvc;

namespace ContosoAds.Web.Pages.Ads;

public class IndexModel : PageModel
{
    private readonly AdsContext _context;
    private readonly ILogger _logger;

    public IndexModel(AdsContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IList<Ad> Ads { get; set; } = Array.Empty<Ad>();

    public async Task OnGetAsync(Category? category, [FromServices] ListAds command)
    {
        _logger.LogDebug("Ads will be displayed for category {Category}", category.HasValue ? category.ToString() : "All");
        Ads = await command.ExecuteAsync(category);
    }
}