using Microsoft.AspNetCore.Http;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;

public sealed class ServiceValidationException : InvalidOperationException, IServiceExceptionContract
{
    public ServiceValidationException(string message) : base(message) { }
    public int StatusCode => StatusCodes.Status400BadRequest;
}
