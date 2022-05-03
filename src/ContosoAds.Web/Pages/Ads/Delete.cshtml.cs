using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.Pages.Ads;

public class DeleteModel : PageModel
{
    private readonly ILogger _logger;

    public DeleteModel(ILogger<DeleteModel> logger)
    {
        _logger = logger;
    }

    public Ad Ad { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, [FromServices] ReadAd command)
    {
        _logger.LogDebug("Ad '{AdId}' will be prompted for deletion", id);
        var ad = await command.ExecuteAsync(id);

        if (ad is null)
        {
            _logger.LogDebug("Ad '{AdId}' not found", id);
            return NotFound();
        }

        Ad = ad;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, [FromServices] DeleteAd command)
    {
        _logger.LogDebug("Ad '{AdId}' will be deleted", id);
        var isDeleted = await command.ExecuteAsync(id);

        if (!isDeleted)
        {
            _logger.LogDebug("Ad '{AdId}' has not been deleted", id);
            return NotFound();
        }

        _logger.LogDebug("Ad '{AdId}' deleted", id);
        return RedirectToPage("./Index");
    }
}