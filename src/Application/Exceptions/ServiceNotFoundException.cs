using Microsoft.AspNetCore.Http;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;

public sealed class ServiceNotFoundException : KeyNotFoundException, IServiceExceptionContract
{
    public ServiceNotFoundException(string message) : base(message) { }
    public int StatusCode => StatusCodes.Status404NotFound;
}
