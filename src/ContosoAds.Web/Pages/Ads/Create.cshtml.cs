using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoAds.Web.Commands;
using ContosoAds.Web.Model;

namespace ContosoAds.Web.Pages.Ads;

public class CreateModel(ILogger<CreateModel> logger) : PageModel
{
    private const int MaxImageSize = 4_194_304;

    public Ad Ad { get; init; } = new();

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    [BindProperty] public IFormFile? ImageFile { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync([FromServices] CreateOrEditAd command)
    {
        logger.LogDebug("New ad will be created");
        // Since we don't use a view model for form input, we need to prevent over-posting.
        // For more details see https://aka.ms/RazorPagesCRUD.
        if (!await TryUpdateModelAsync(
                Ad,
                "ad",
                x => x.Category,
                x => x.Description!,
                x => x.Price,
                x => x.Phone,
                x => x.Title))
        {
            logger.LogDebug("Ad failed input validation");
            return Page();
        }

        if (ImageFile is { Length: > MaxImageSize })
        {
            logger.LogDebug("Image exceeds maximum byte size: {ActualSize} > {MaxSize}", ImageFile!.Length, MaxImageSize);
            ModelState.AddModelError("ImageFile", $"Image file size must be less than {MaxImageSize} bytes");
            return Page();
        }

        // We ignore the return value here, since any completion of ExecuteAsync() for the "Create" use case
        // results in true being returned.
        await command.ExecuteAsync(Ad, ImageFile);
        logger.LogDebug("Ad '{AdId}' created", Ad.Id);
        return RedirectToPage("./Index");
    }
}