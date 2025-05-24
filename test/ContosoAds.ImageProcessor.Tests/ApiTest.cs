using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Dapr;
using Dapr.Client;

namespace ContosoAds.ImageProcessor.Tests;

public class ApiTest(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Theory]
    [InlineData("/healthz/live")]
    [InlineData("/healthz/ready")]
    public async Task Probe_Returns_OK(string uri)
    {
        // Arrange
        using var client = factory.CreateClient();

        // Assert
        using var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        // Act
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Options_Returns_OK()
    {
        // Arrange
        using var client = factory.CreateClient();

        // Assert
        using var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Options, "/thumbnail-request"),
            TestContext.Current.CancellationToken);

        // Act
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Returns_Ok()
    {
        // Arrange
        var bindingRequest = new BindingRequest("fake", "fake");
        var bindingResponse = new BindingResponse(
            bindingRequest,
            JpegBytes,
            A.Fake<IReadOnlyDictionary<string, string>>());
        A.CallTo(() => factory.DaprClient.InvokeBindingAsync(
                A<BindingRequest>._, 
                A<CancellationToken>._))
            .Returns(Task.FromResult(bindingResponse));
        A.CallTo(() => factory.DaprClient.InvokeBindingAsync<byte[], JsonNode>(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<IReadOnlyDictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.FromResult(CreateBlobResponse("tn-foo.jpg")));

        using var client = factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-request",
            new ImageBlob
            {
                Uri = new Uri("https://example.com/foo.jpg"),
                AdId = 1
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingImage_Returns_Ok()
    {
        // Arrange
        A.CallTo(() => factory.DaprClient.InvokeBindingAsync(
                A<BindingRequest>._, 
                A<CancellationToken>._))
            .Throws<DaprApiException>();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-request",
            new ImageBlob
            {
                Uri = new Uri("https://example.com/foo.jpg"),
                AdId = 1
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("http://some/image.jpg", null)]
    [InlineData("", 1)]
    [InlineData("image.jpg", 1)]
    [InlineData(null, 1)]
    [InlineData(null, null)]

    public async Task Post_WithInvalidMessageContent_Returns_BadRequest(string? url, int? adId)
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-request",
            new {Uri = url, AdId = adId},
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}