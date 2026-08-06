using UrbanSync.Api.Middleware;

namespace UrbanSync.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(
        this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        var swaggerEnabled =
            app.Environment.IsDevelopment() ||
            app.Configuration.GetValue<bool>("Swagger:Enabled");

        if (swaggerEnabled)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "UrbanSync API v1");

                options.RoutePrefix = "swagger";
            });
        }

        app.UseCors(ServiceCollectionExtensions.CorsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}