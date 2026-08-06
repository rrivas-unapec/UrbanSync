using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UrbanSync.Infrastructure.Authentication;

namespace UrbanSync.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "UrbanSyncCors";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                "Bearer",
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description =
                        "Introduce el token JWT sin escribir la palabra Bearer."
                });

            options.AddSecurityRequirement(
                new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference =
                                new Microsoft.OpenApi.Models.OpenApiReference
                                {
                                    Type =
                                        Microsoft.OpenApi.Models.ReferenceType
                                            .SecurityScheme,
                                    Id = "Bearer"
                                }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        services.AddHealthChecks();

        ConfigureValidationResponses(services);
        ConfigureCors(services, configuration);
        ConfigureAuthentication(services, configuration);

        services.AddAuthorization();

        return services;
    }

    private static void ConfigureValidationResponses(
        IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error =>
                                string.IsNullOrWhiteSpace(
                                    error.ErrorMessage)
                                    ? "El valor proporcionado no es válido."
                                    : error.ErrorMessage)
                            .ToArray());

                var problemDetails =
                    new ValidationProblemDetails(errors)
                    {
                        Status =
                            StatusCodes.Status400BadRequest,
                        Title =
                            "La solicitud contiene datos inválidos.",
                        Instance =
                            context.HttpContext.Request.Path
                    };

                problemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problemDetails);
            };
        });
    }

    private static void ConfigureCors(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    return;
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    private static void ConfigureAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetRequiredSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "No se pudo cargar la configuración JWT.");

        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) ||
            jwtOptions.SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SecretKey debe contener al menos 32 caracteres.");
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.FromMinutes(1),

                        NameClaimType = ClaimTypes.Name,

                        RoleClaimType = ClaimTypes.Role
                    };
            });
    }
}