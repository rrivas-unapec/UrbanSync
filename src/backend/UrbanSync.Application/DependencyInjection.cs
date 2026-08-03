using Microsoft.Extensions.DependencyInjection;
using UrbanSync.Application.Features.Roles;
using UrbanSync.Application.Features.Users;

namespace UrbanSync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        return services;
    }
}