using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.Pages.Ads;

public class DetailsModel(ILogger<DetailsModel> logger) : PageModel
{
    public Ad Ad { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, [FromServices] ReadAd command)
    {
        logger.LogDebug("Ad '{AdId}' will be displayed in detail", id);
        var ad = await command.ExecuteAsync(id);
            
        if (ad is null)
        {
            logger.LogDebug("Ad '{AdId}' cannot be displayed", id);
            return NotFound();
        }

        Ad = ad;
        logger.LogDebug("Displaying Ad '{AdId}'", id);
        return Page();
    }
}