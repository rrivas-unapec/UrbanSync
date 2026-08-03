using UrbanSync.Web.ApiClients.Authentication;
using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.ApiClients.Reports;
using UrbanSync.Web.ApiClients.Roles;
using UrbanSync.Web.ApiClients.Users;
using UrbanSync.Web.ApiClients.WorkOrders;

namespace UrbanSync.Web.Extensions;

public static class ApiClientServiceCollectionExtensions
{
    public static IServiceCollection AddUrbanSyncApiClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["UrbanSyncApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "UrbanSyncApi:BaseUrl no está configurado.");

        services.AddUrbanSyncHttpClient<
            IAuthenticationApiClient,
            AuthenticationApiClient>(baseUrl);

        services.AddUrbanSyncHttpClient<
            IUsersApiClient,
            UsersApiClient>(baseUrl);

        services.AddUrbanSyncHttpClient<
            IRolesApiClient,
            RolesApiClient>(baseUrl);

        services.AddUrbanSyncHttpClient<
            IReportsApiClient,
            ReportsApiClient>(baseUrl);

        services.AddUrbanSyncHttpClient<
            IIncidentsApiClient,
            IncidentsApiClient>(baseUrl);

        services.AddUrbanSyncHttpClient<
            IWorkOrdersApiClient,
            WorkOrdersApiClient>(baseUrl);

        return services;
    }

    private static void AddUrbanSyncHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string baseUrl)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }
}