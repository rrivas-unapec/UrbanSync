using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrbanSync.Application.Common.Interfaces.Authentication;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Infrastructure.Authentication;
using UrbanSync.Infrastructure.Persistence.Connections;
using UrbanSync.Infrastructure.Persistence.Repositories;

namespace UrbanSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("UrbanSyncDb")
            ?? throw new InvalidOperationException(
                "Connection string 'UrbanSyncDb' no configurada.");

        services.AddSingleton<IDbConnectionFactory>(
            new SqlConnectionFactory(connectionString));

        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}