using Azure.Core;
using Azure.Identity;

namespace ContosoAds.Web;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddNpgsqlDataSource(
        this IServiceCollection services,
        string connectionString,
        bool useEntraId,
        string? managedIdentityClientId = null)
    {
        services.AddNpgsqlDataSource(connectionString,
            dataSourceBuilder =>
            {
                dataSourceBuilder.Name = nameof(ContosoAds);
                if (!useEntraId) return;
                
                var credential = GetCredential(managedIdentityClientId);
                dataSourceBuilder.UsePeriodicPasswordProvider(
                    async (_, cancellationToken) =>
                    {
                        var accessToken = await credential.GetTokenAsync(
                            new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]),
                            cancellationToken);
                        return accessToken.Token;
                    },
                    TimeSpan.FromMinutes(55), // Interval for refreshing the token
                    TimeSpan.FromSeconds(5)); // Interval for retrying after a refresh failure
            });

        return services;
    }

    private static DefaultAzureCredential GetCredential(string? managedIdentityClientId)
    {
        var options = new DefaultAzureCredentialOptions
        {
#if !DEBUG
                    ExcludeInteractiveBrowserCredential = true,
                    ExcludeVisualStudioCredential = true,
                    ExcludeVisualStudioCodeCredential = true,
                    ExcludeAzureCliCredential = true,
                    ExcludeAzurePowerShellCredential = true,
                    ExcludeAzureDeveloperCliCredential = true,
                    ExcludeSharedTokenCacheCredential = true
#endif
        };
        if (managedIdentityClientId is { Length: > 0 })
        {
            options.ManagedIdentityClientId = managedIdentityClientId;
        }

        return new DefaultAzureCredential(options);
    }
}