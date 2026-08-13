using UrbanSync.Web.Presentation.Assets;
using UrbanSync.Web.Presentation.Claims;
using UrbanSync.Web.Presentation.Dashboard;
using UrbanSync.Web.Presentation.Departments;
using UrbanSync.Web.Presentation.Institutions;
using UrbanSync.Web.Presentation.Jurisdictions;
using UrbanSync.Web.Presentation.Locations;
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

        services.AddScoped<
            IClaimsPageService,
            ClaimsPageService>();

        services.AddScoped<
            IJurisdictionsPageService,
            JurisdictionsPageService>();

        services.AddScoped<
            IDepartmentsPageService,
            DepartmentsPageService>();

        services.AddScoped<
            IInstitutionsPageService,
            InstitutionsPageService>();

        services.AddScoped<
            ILocationsPageService,
            LocationsPageService>();

        services.AddScoped<
            IAssetsPageService,
            AssetsPageService>();

        return services;
    }
}