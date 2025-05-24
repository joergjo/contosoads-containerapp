using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace ContosoAds.Web.Tests.Pages.Ads;

public class CreateTest(PostgreSqlContainerFixture fixture) : PageTestBase(fixture)
{
    [Theory]
    [InlineData("/ads/create")]
    [InlineData("/ads/create/")]
    public async Task Get_Returns_OkAndPage(string uri)
    {
        // Arrange
        await WebApplicationFactory.SeedDatabaseAsync();
        using var client = WebApplicationFactory.CreateClient();

        // Act
        using var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        // Assert
        using var document = await response.ToDocumentAsync();
        var title = document.QuerySelector("title")?.TextContent;
        Assert.NotNull(title);
        Assert.StartsWith("Create Ad", title);
    }

    [Theory]
    [InlineData("/ads/create/9999")]
    [InlineData("/ads/create/abc")]
    [InlineData("/ads/create/more/than/one/part")]
    [InlineData("/ads/create/ad/1")]
    public async Task Get_WithAdditionalSegments_Returns_NotFound(string uri)
    {
        // Arrange
        await WebApplicationFactory.SeedDatabaseAsync();
        using var client = WebApplicationFactory.CreateClient();

        // Act
        using var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, uri), 
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task Post_RedirectsTo_Ads()
    {
        // Arrange
        const string uri = "/ads/create";
        await WebApplicationFactory.SeedDatabaseAsync();
        var client = WebApplicationFactory.CreateClient();
        using var getResponse = await client.GetAsync(uri, TestContext.Current.CancellationToken);
        using var document = await getResponse.ToDocumentAsync();
        var csrfToken = document.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("Ad.Category", "Cars"),
                new KeyValuePair<string, string>("Ad.Description", "Test Description"),
                new KeyValuePair<string, string>("Ad.Phone", "425-555-1212"),
                new KeyValuePair<string, string>("Ad.Price", "10000"),
                new KeyValuePair<string, string>("Ad.Title", "Test Ad"),
                new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken!)
            ])
        };
        using var postResponse = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/ads", postResponse.Headers.Location?.ToString().ToLower());
    }
    
    [Fact]
    public async Task Post_MultiPartFormData_RedirectsTo_Ads()
    {
        // Arrange
        var bindingRequest = new BindingRequest("fake", "fake");
        var bindingResponse = new BindingResponse(
            bindingRequest,
            JpegBytes,
            A.Fake<IReadOnlyDictionary<string, string>>());
        A.CallTo(() => WebApplicationFactory.DaprClient.InvokeBindingAsync(
                A<BindingRequest>._, 
                A<CancellationToken>._))
            .Returns(Task.FromResult(bindingResponse));
        A.CallTo(() => WebApplicationFactory.DaprClient.InvokeBindingAsync<byte[], JsonNode>(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<IReadOnlyDictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.FromResult(CreateBlobResponse("test.jpg")));

        const string uri = "/ads/create";
        await WebApplicationFactory.SeedDatabaseAsync();
        var client = WebApplicationFactory.CreateClient();
        using var getResponse = await client.GetAsync(uri, TestContext.Current.CancellationToken);
        using var document = await getResponse.ToDocumentAsync();
        var csrfToken = document.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value");

        // Act
        var multipartContent = new MultipartFormDataContent
        {
            { new StringContent("Cars"), "Ad.Category" },
            { new StringContent("Test Description"), "Ad.Description" },
            { new StringContent("425-555-1212"), "Ad.Phone" },
            { new StringContent("10000"), "Ad.Price" },
            { new StringContent("Test Ad"), "Ad.Title" },
            { new StringContent(csrfToken!), "__RequestVerificationToken" }
        };

        var fileContent = new ByteArrayContent(Convert.FromBase64String(Jpeg));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        multipartContent.Add(fileContent, "ImageFile", "test.jpg");
        
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = multipartContent
        };
        using var postResponse = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/ads", postResponse.Headers.Location?.ToString().ToLower());
    }
}