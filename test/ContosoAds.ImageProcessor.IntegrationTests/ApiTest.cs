using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using FakeItEasy;
using static ContosoAds.ImageProcessor.IntegrationTests.TestSupport;

namespace ContosoAds.ImageProcessor.IntegrationTests;

public class ApiTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ApiTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Live_Returns_OK()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Assert
        using var response = await client.GetAsync("/healthz/live");

        // Act
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Options_Returns_OK()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Assert
        using var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Options, "/thumbnail-request"));

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
        A.CallTo(() => _factory.DaprClient.InvokeBindingAsync(A<BindingRequest>._, A<CancellationToken>._))
            .Returns(Task.FromResult(bindingResponse));
        A.CallTo(() => _factory.DaprClient.InvokeBindingAsync<byte[], JsonNode>(
                A<string>._,
                A<string>._,
                A<byte[]>._,
                A<IReadOnlyDictionary<string, string>>._,
                A<CancellationToken>._))
            .Returns(Task.FromResult(CreateBlobResponse("tn-foo.jpg")));

        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-request",
            new ImageBlob(new Uri("https://example.com/foo.jpg"), 1));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingImage_Returns_Ok()
    {
        // Arrange
        A.CallTo(() => _factory.DaprClient.InvokeBindingAsync(A<BindingRequest>._, A<CancellationToken>._))
            .Throws<DaprApiException>();
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-request",
            new ImageBlob(new Uri("https://example.com/foo.jpg"), 1));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidMessageContent_Returns_BadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync(
            "/thumbnail-request",
            new {Uri = default(string), AdId = 0});

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}