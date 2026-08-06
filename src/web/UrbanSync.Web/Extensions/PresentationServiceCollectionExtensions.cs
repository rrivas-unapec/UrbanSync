using UrbanSync.Web.Presentation.Dashboard;
using UrbanSync.Web.Presentation.Users;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Extensions;

public static class PresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services)
    {
        services.AddScoped<ActivityLogger>();

        services.AddScoped<
            IDashboardPageService,
            DashboardPageService>();

        services.AddScoped<
            IUserManagementPageService,
            UserManagementPageService>();

        return services;
    }
}