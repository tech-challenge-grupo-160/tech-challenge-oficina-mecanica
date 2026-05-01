using Fiap.TechChallenge.OficinaMecanica.API.Contracts;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fiap.TechChallenge.OficinaMecanica.API.Filters;

public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        if (exception is IServiceExceptionContract serviceException)
        {
            context.Result = new ObjectResult(CreateResponse(
                exception.Message,
                context))
            {
                StatusCode = serviceException.StatusCode
            };
            context.ExceptionHandled = true;
            return;
        }

        if (exception is KeyNotFoundException)
        {
            context.Result = new ObjectResult(CreateResponse(
                exception.Message,
                context))
            {
                StatusCode = StatusCodes.Status404NotFound
            };
            context.ExceptionHandled = true;
            return;
        }

        if (exception is ArgumentException or InvalidOperationException)
        {
            context.Result = new ObjectResult(CreateResponse(
                exception.Message,
                context))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        if (exception is UnauthorizedAccessException)
        {
            context.Result = new ObjectResult(CreateResponse(
                exception.Message,
                context))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            context.ExceptionHandled = true;
            return;
        }

        context.Result = new ObjectResult(CreateResponse(
            "Erro interno no servidor.",
            context))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }

    private static ErrorResponseContract CreateResponse(string message, ExceptionContext context)
    {
        return new ErrorResponseContract
        {
            Message = message,
            TraceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier
        };
    }
}
