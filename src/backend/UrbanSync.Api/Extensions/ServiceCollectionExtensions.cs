using Microsoft.AspNetCore.Mvc;

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
        services.AddSwaggerGen();

        services.AddHealthChecks();

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
                                string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "El valor proporcionado no es válido."
                                    : error.ErrorMessage)
                            .ToArray());

                var problemDetails = new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "La solicitud contiene datos inválidos.",
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problemDetails);
            };
        });

        var allowedOrigins =
            configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
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

        return services;
    }
}