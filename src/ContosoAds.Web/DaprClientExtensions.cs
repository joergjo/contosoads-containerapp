﻿using System.Text.Json.Nodes;
using Dapr.Client;

namespace ContosoAds.Web;

public static class DaprClientExtensions
{
    public static async Task<string?> WriteAzureBlobAsync(this DaprClient daprClient, string bindingName,
        string blobName, byte[] data, string? contentType = default)
    {
        var request = new BindingRequest(bindingName, "create")
        {
            Data = data
        };
        request.Metadata.Add("blobName", blobName);
        if (contentType is not null)
        {
            request.Metadata.Add("contentType", contentType);
        }

        var response = await daprClient.InvokeBindingAsync(request);
        return response.Metadata["blobURL"];
    }
    
    public static async Task<string?> WriteAzureBlobBase64Async(this DaprClient daprClient, string bindingName,
        string blobName, byte[] data, string? contentType = default)
    {
        var metadata = new Dictionary<string, string>
        {
            {"blobName", blobName}
        };
        if (contentType is { Length: > 0 })
        {
            metadata.Add("contentType", contentType);
        }

        var json = await daprClient.InvokeBindingAsync<byte[], JsonNode>(bindingName, "create", data, metadata);
        return json?["blobURL"]?.GetValue<string>();
    }
}