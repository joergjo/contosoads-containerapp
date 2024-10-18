using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ContosoAds.Web.TagHelpers;

public class ImgTagHelper : TagHelper
{
    private const string SrcAttributeName = "src";
    private const string HostAttributeName = "host";
    private const string PortAttributeName = "port";

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Src { get; set; } = default!;

    public string? Host { get; set; }

    public int? Port { get; set; }
    
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string OriginalHost { get; set; } = "host.docker.internal";

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    public override int Order { get; } = 100;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.CopyHtmlAttribute(SrcAttributeName, context);
        if (Host is not { Length: > 0 } && Port is null) return;
        
        var src = RewriteSrc(Src, OriginalHost, Host, Port);
        output.Attributes.SetAttribute(SrcAttributeName, src);
    }

    private static string RewriteSrc(string src, string originalHost, string? host, int? port)
    {
        // If we cannot parse an absolute URL, we leave src as is.
        if (!Uri.TryCreate(src, UriKind.Absolute, out var uri))
        {
            return src;
        }

        // If the host does not match original host, we leave src as is.
        if (uri.Host != originalHost)
        {
            return src;
        }
        
        // Override the host and port if they are provided.
        var builder = new UriBuilder(uri)
        {
            Host = host ?? uri.Host,
            Port = port ?? uri.Port
        };

        return builder.Uri.ToString();
    }
}