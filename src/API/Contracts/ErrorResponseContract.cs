namespace Fiap.TechChallenge.OficinaMecanica.API.Contracts;

public sealed class ErrorResponseContract
{
    public required string Message { get; init; }
    public required string TraceId { get; init; }
}
