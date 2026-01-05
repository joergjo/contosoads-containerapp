using Azure.Core;
using Azure.Identity;

namespace ContosoAds.Web;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNpgsqlDataSource(string connectionString,
            TokenCredential? credential,
            int refreshIntervalMinutes = 55,
            int retryIntervalSeconds = 5)
        {
            services.AddNpgsqlDataSource(connectionString,
                dataSourceBuilder =>
                {
                    dataSourceBuilder.Name = nameof(ContosoAds);
                    if (credential is null) return;
                    
                    dataSourceBuilder.UsePeriodicPasswordProvider(
                        async (_, cancellationToken) =>
                        {
                            var accessToken = await credential.GetTokenAsync(
                                new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]),
                                cancellationToken);
                            return accessToken.Token;
                        },
                        TimeSpan.FromMinutes(refreshIntervalMinutes), // Interval for refreshing the token
                        TimeSpan.FromSeconds(retryIntervalSeconds)); // Interval for retrying after a refresh failure
                });

            return services;
        }
    }
}