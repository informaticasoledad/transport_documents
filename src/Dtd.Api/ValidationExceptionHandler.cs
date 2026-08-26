using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dtd.Api;

/// <summary>Maps <see cref="ValidationException"/> (thrown by the validation pipeline behavior) to a 400 ProblemDetails.</summary>
internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var errors = validationException.Errors
            .Select(e => new { field = e.PropertyName, error = e.ErrorMessage })
            .ToArray();

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Solicitud no válida",
            Detail = "Uno o más campos no superaron la validación.",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["errors"] = errors;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}