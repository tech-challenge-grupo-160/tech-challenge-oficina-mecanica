using System.Net;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;

public sealed class ServiceValidationException : InvalidOperationException, IServiceExceptionContract
{
    public ServiceValidationException(string message) : base(message) { }
    public int StatusCode => (int)HttpStatusCode.BadRequest;
}
