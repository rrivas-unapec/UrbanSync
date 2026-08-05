using UrbanSync.Web.ApiClients.Authentication;
using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.ApiClients.Reports;
using UrbanSync.Web.ApiClients.Roles;
using UrbanSync.Web.ApiClients.Users;
using UrbanSync.Web.ApiClients.WorkOrders;
using UrbanSync.Web.Authentication;

namespace UrbanSync.Web.Extensions;

public static class ApiClientServiceCollectionExtensions
{
    public static IServiceCollection AddUrbanSyncApiClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configuredBaseUrl =
            configuration["UrbanSyncApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "UrbanSyncApi:BaseUrl no está configurado.");

        var normalizedBaseUrl =
            configuredBaseUrl.EndsWith(
                "/",
                StringComparison.Ordinal)
                ? configuredBaseUrl
                : $"{configuredBaseUrl}/";

        services.AddTransient<AccessTokenHandler>();

        services.AddUrbanSyncHttpClient<
            IAuthenticationApiClient,
            AuthenticationApiClient>(normalizedBaseUrl);

        services.AddUrbanSyncHttpClient<
            IUsersApiClient,
            UsersApiClient>(normalizedBaseUrl);

        services.AddUrbanSyncHttpClient<
            IRolesApiClient,
            RolesApiClient>(normalizedBaseUrl);

        services.AddUrbanSyncHttpClient<
            IReportsApiClient,
            ReportsApiClient>(normalizedBaseUrl);

        services.AddUrbanSyncHttpClient<
            IIncidentsApiClient,
            IncidentsApiClient>(normalizedBaseUrl);

        services.AddUrbanSyncHttpClient<
            IWorkOrdersApiClient,
            WorkOrdersApiClient>(normalizedBaseUrl);

        return services;
    }

    private static void AddUrbanSyncHttpClient<
        TClient,
        TImplementation>(
        this IServiceCollection services,
        string baseUrl)
        where TClient : class
        where TImplementation : class, TClient
    {
        services
            .AddHttpClient<TClient, TImplementation>(
                client =>
                {
                    client.BaseAddress = new Uri(baseUrl);

                    client.Timeout =
                        TimeSpan.FromSeconds(30);
                })
            .AddHttpMessageHandler<AccessTokenHandler>();
    }
}