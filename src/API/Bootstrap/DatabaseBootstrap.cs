using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Extensions;

namespace Fiap.TechChallenge.OficinaMecanica.API.Bootstrap;

public static class DatabaseBootstrap
{
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
        var usingInMemoryDatabase = app.Environment.IsEnvironment("Testing") ||
            string.Equals(connectionString, "UseInMemory", StringComparison.OrdinalIgnoreCase);

        if (!usingInMemoryDatabase)
        {
            await app.MigrateAndSeedAsync(app.Environment.IsDevelopment());
        }

        return app;
    }
}
