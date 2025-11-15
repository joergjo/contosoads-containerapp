using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;
using Microsoft.AspNetCore.Mvc;

namespace ContosoAds.Web.Pages.Ads;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public IList<Ad> Ads { get; set; } = Array.Empty<Ad>();

    public async Task OnGetAsync(Category? category, [FromServices] ListAds command)
    {
        logger.LogDebug("Ads will be displayed for category {Category}", category.HasValue ? category.ToString() : "All");
        Ads = await command.ExecuteAsync(category);
    }

    public async Task<IActionResult> OnGetImageAsync(int id, [FromServices] ReadAd command)
    {
        logger.LogDebug("Rendering thumbnail for Ad {AdId}", id);
        var ad = await command.ExecuteAsync(id);
        return Partial("_Thumbnail", ad);
    }
}