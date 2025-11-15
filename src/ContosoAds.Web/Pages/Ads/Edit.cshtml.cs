using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.Pages.Ads;

public class EditModel(ILogger<EditModel> logger) : PageModel
{
    public Ad Ad { get; set; } = new();

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    [BindProperty] public IFormFile? ImageFile { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, [FromServices] ReadAd handler)
    {
        logger.LogDebug("Ad {AdID} will be displayed for editing", id);
        var ad = await handler.ExecuteAsync(id);

        if (ad is null)
        {
            logger.LogDebug("Ad '{AdId}' not found", id);
            return NotFound();
        }

        Ad = ad;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync(int id, [FromServices] CreateOrEditAd command)
    {
        logger.LogDebug("Ad {AdID} will be updated", id);
        if (!await TryUpdateModelAsync(
                Ad,
                "ad",
                x => x.Id,
                x => x.Category,
                x => x.Description!,
                x => x.Phone!,
                x => x.Price,
                x => x.Title))
        {
            logger.LogDebug("Ad {AdID} failed input validation", id);
            return Page();
        }

        if (!await command.ExecuteAsync(Ad, ImageFile))
        {
            logger.LogDebug("Ad '{AdId}' failed to be updated", id);
            return NotFound();
        }

        logger.LogDebug("Ad '{AdId}' updated", id);
        return RedirectToPage("./Index");
    }
}