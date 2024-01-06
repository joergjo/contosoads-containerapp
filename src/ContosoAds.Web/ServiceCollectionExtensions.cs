using Azure.Core;
using Azure.Identity;

namespace ContosoAds.Web;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddNpgsqlDataSource(
        this IServiceCollection services,
        string connectionString,
        bool useEntraId = false)
    {
        services.AddNpgsqlDataSource(connectionString,
            dataSourceBuilder =>
            {
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
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
                });
                dataSourceBuilder.Name = nameof(ContosoAds);
                if (useEntraId)
                {
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
                }
            });

        return services;
    }
}