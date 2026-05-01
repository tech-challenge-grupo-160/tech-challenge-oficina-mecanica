using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Logging;

public sealed class PlainConsoleFormatter : ConsoleFormatter
{
    public const string FormatterName = "plain";
    private readonly IDisposable? _optionsReloadToken;
    private PlainConsoleFormatterOptions _options;

    public PlainConsoleFormatter(IOptionsMonitor<PlainConsoleFormatterOptions> options)
        : base(FormatterName)
    {
        _options = options.CurrentValue;
        _optionsReloadToken = options.OnChange(updated => _options = updated);
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message) && logEntry.Exception is null)
        {
            return;
        }

        var timestamp = _options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        var timestampText = string.IsNullOrWhiteSpace(_options.TimestampFormat)
            ? timestamp.ToString("O")
            : timestamp.ToString(_options.TimestampFormat);

        var level = ToLevelLabel(logEntry.LogLevel);
        var line = new StringBuilder();
        line.Append(timestampText);
        line.Append(level);
        line.Append(": ");
        line.Append(message);

        if (logEntry.Exception is not null)
        {
            line.Append(' ');
            line.Append(SingleLine(logEntry.Exception.ToString()));
        }

        textWriter.WriteLine(line.ToString());
    }

    private static string SingleLine(string value)
    {
        return value
            .Replace("\r\n", " ")
            .Replace('\n', ' ')
            .Replace('\r', ' ');
    }

    private static string ToLevelLabel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => "none"
        };
    }

}
