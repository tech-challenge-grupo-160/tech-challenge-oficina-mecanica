using Fiap.TechChallenge.OficinaMecanica.Infrastructure.HealthChecks;
using Fiap.TechChallenge.OficinaMecanica.API.Middleware;

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

        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();
        app.UseHealthChecks();

        return app;
    }
}
