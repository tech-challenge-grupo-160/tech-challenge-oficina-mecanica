namespace Fiap.TechChallenge.OficinaMecanica.Shared.Logging;

public static class LogTemplate
{
    public const string Start = "[Iniciando] service: {service}.";
    public const string End = "[Finalizando] service: {service}. | {message}";
    public const string Trace = "[Executando] -> service: {service}, method: {method} | {message}.";
    public const string Error = "[Erro] service: {service}, method: {method} | {message}.";
    public const string Warning = "[Warning] service: {service}, method: {method} | {message}.";
}
