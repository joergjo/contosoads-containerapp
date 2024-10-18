using ContosoAds.Web.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoAds.Web;

public class ImgTagHelperInitializer : ITagHelperInitializer<ImgTagHelper>
{
    private readonly string? _host;
    private readonly int? _port;
    
    public ImgTagHelperInitializer(string? host, int? port)
    {
        _host = host;
        _port = port;
    }
    public void Initialize(ImgTagHelper helper, ViewContext context)
    {
        helper.Host = _host;
        helper.Port = _port;
    }
}