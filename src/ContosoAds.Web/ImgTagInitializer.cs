using ContosoAds.Web.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoAds.Web;

public class ImgTagHelperInitializer(string? host, int? port) : ITagHelperInitializer<ImgTagHelper>
{
    public void Initialize(ImgTagHelper helper, ViewContext context)
    {
        helper.Host = host;
        helper.Port = port;
    }
}