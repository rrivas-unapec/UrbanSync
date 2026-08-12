using Microsoft.Extensions.DependencyInjection;
using UrbanSync.Application.Features.Asset;
using UrbanSync.Application.Features.Audit;
using UrbanSync.Application.Features.Incidents;
using UrbanSync.Application.Features.IncidentType;
using UrbanSync.Application.Features.Roles;
using UrbanSync.Application.Features.Users;
using UrbanSync.Application.Features.Jurisdiction;
using UrbanSync.Application.Features.Departament;
using UrbanSync.Application.Features.Institution;
using UrbanSync.Application.Features.Location;

namespace UrbanSync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<
            IRolService,
            RolService>();

        services.AddScoped<
            IUsuarioService,
            UsuarioService>();

        services.AddScoped<
            IIncidentService,
            IncidentService>();

        services.AddScoped<
            IIncidentNotificationService,
            IncidentNotificationService>();

        services.AddScoped<
            IAuditService,
            AuditService>();

        services.AddScoped<
            IIncidentTypeService,
            IncidentTypeService>();

        services.AddScoped<
            IAssetService,
            AssetService>();

        services.AddScoped<
            IJurisdictionService,
            JurisdictionService>();

        services.AddScoped<
            IDepartmentService,
            DepartmentService>();

        services.AddScoped<
            IInstitutionService,
            InstitutionService>();

        services.AddScoped<
            ILocationService,
            LocationService>();

        return services;
    }
}