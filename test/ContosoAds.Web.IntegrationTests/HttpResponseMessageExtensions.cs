using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace ContosoAds.Web.IntegrationTests;

public static class HttpResponseMessageExtensions
{
    public static async Task<IDocument> ToDocumentAsync(this HttpResponseMessage response)
    {
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(request =>
        {
            request
                .Content(response.Content.ReadAsStream(), shouldDispose: true)
                .Address(response.RequestMessage?.RequestUri)
                .Status(response.StatusCode);
        });
    }
}