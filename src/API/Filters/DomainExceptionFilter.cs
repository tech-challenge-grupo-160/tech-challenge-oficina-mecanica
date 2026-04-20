using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fiap.TechChallenge.OficinaMecanica.API.Filters;

public class DomainExceptionFilter : IExceptionFilter
{
    private static readonly HashSet<Type> BadRequestExceptions = new()
    {
        typeof(ArgumentException),
        typeof(InvalidOperationException)
    };

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is KeyNotFoundException notFound)
        {
            context.Result = new NotFoundObjectResult(new { message = notFound.Message });
            context.ExceptionHandled = true;
            return;
        }

        if (BadRequestExceptions.Contains(context.Exception.GetType()))
        {
            context.Result = new BadRequestObjectResult(new { message = context.Exception.Message });
            context.ExceptionHandled = true;
        }
    }
}
