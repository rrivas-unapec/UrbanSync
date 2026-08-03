using UrbanSync.Api.Middleware;

namespace UrbanSync.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(
        this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(ServiceCollectionExtensions.CorsPolicyName);

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}