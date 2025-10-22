using System.Net.Http.Headers;
using JasperFx.Core;
using Oakton;

namespace ContosoAds.Cli;

[Description("Creates multiple ads")]
public class CreateAdCommand : OaktonAsyncCommand<CreateAdInput>
{
    public CreateAdCommand()
    {
        Usage("Quickly create single ad").Arguments(x => x.Uri);
        Usage("Quickly create multiple ads").Arguments(x => x.Uri, x => x.Count);
        Usage("Create multiple ads with timeout per operation").Arguments(x => x.Uri, x => x.Count, x => x.Timeout);
        Usage("Create multiple ads with custom image file").Arguments(x => x.Uri, x => x.Count, x => x.ImageFile);
        Usage("Create multiple ads with custom image file and MIME type")
            .Arguments(x => x.Uri, x => x.Count, x => x.ImageFile, x => x.MimeType);
    }

    public override async Task<bool> Execute(CreateAdInput input)
    {
        using var client = new HttpClient();
        var adsUri = new Uri(input.Uri, "/ads/create");

        for (var i = 0; i < input.Count; i++)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(input.Timeout));
            using var getResponse = await client.GetAsync(adsUri, cts.Token);
            using var document = await getResponse.ToDocumentAsync();
            var csrfToken = document.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value");

            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StringContent("Cars"), "Ad.Category");
            multipartContent.Add(new StringContent($"Test Description {i}"), "Ad.Description");
            multipartContent.Add(new StringContent("425-555-1212"), "Ad.Phone");
            multipartContent.Add(new StringContent("10000"), "Ad.Price");
            multipartContent.Add(new StringContent($"Test Ad {i}"), "Ad.Title");
            multipartContent.Add(new StringContent(csrfToken!), "__RequestVerificationToken");

            var fileStream = File.OpenRead(input.ImageFile);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(input.MimeType);
            multipartContent.Add(fileContent, "ImageFile", input.ImageFile);

            var request = new HttpRequestMessage(HttpMethod.Post, adsUri)
            {
                Content = multipartContent
            };

            using var postResponse = await client.SendAsync(request, cts.Token);
            if (postResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Post succeeded");
            }
            else
            {
                Console.WriteLine("Post failed: {0}", postResponse.StatusCode);
                Console.WriteLine("Details: {0}", await postResponse.Content.ReadAsStringAsync(cts.Token));
                var content = await postResponse.Content.ReadAsStringAsync(cts.Token);
                Console.WriteLine(content);
            }
        }

        return true;
    }
}