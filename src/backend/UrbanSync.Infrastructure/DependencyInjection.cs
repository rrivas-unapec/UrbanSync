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

        services
            .AddOptions<JwtOptions>()
            .Bind(
                configuration.GetRequiredSection(
                    JwtOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer es obligatorio.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience es obligatorio.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.SecretKey) &&
                    options.SecretKey.Length >= 32,
                "Jwt:SecretKey debe contener al menos 32 caracteres.")
            .Validate(
                options => options.ExpirationMinutes > 0,
                "Jwt:ExpirationMinutes debe ser mayor que cero.")
            .ValidateOnStart();

        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}