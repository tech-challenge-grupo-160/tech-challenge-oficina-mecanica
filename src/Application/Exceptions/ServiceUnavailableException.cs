namespace Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;

public sealed class ServiceUnavailableException(string message)
    : Exception(message), IServiceExceptionContract
{
    public int StatusCode => 503;
}
