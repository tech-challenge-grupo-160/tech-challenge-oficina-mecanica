using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Logging;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.API.Bootstrap;

public static class ApiLoggingBootstrap
{
    public static WebApplicationBuilder ConfigureApiLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsoleFormatter<PlainConsoleFormatter, PlainConsoleFormatterOptions>();
        builder.Logging.AddConsole(options =>
        {
            options.FormatterName = PlainConsoleFormatter.FormatterName;
        });
        builder.Logging.Services.Configure<PlainConsoleFormatterOptions>(options =>
        {
            options.TimestampFormat = "dd/MM/yyyy HH:mm:ss ";
            options.UseUtcTimestamp = false;
        });

        return builder;
    }
}
