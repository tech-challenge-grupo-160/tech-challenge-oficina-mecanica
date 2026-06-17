using Fiap.TechChallenge.OficinaMecanica.Infrastructure.HealthChecks;

namespace Fiap.TechChallenge.OficinaMecanica.API.Bootstrap;

public static class ApiPipelineBootstrap
{
    public static WebApplication UseApiPipeline(this WebApplication app, IConfiguration configuration)
    {
        app.UseApiSwagger();

        var configuredUrls = configuration["ASPNETCORE_URLS"] ?? string.Empty;
        var hasHttpsBinding = configuredUrls.Contains("https://", StringComparison.OrdinalIgnoreCase);

        if (hasHttpsBinding)
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseHealthChecks();

        return app;
    }
}
