using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Dtd.Api;

/// <summary>Maps <see cref="ErrorOr{T}"/> results to typed HTTP responses (RFC 9457 ProblemDetails on errors).</summary>
public static class ErrorOrHttpExtensions
{
    public static IResult ToHttpResult<T>(this ErrorOr<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsError)
        {
            return ToProblemDetails(result.Errors);
        }

        return onSuccess(result.Value);
    }

    private static IResult ToProblemDetails(List<Error> errors)
    {
        var first = errors[0];
        var (status, title) = first.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Solicitud no válida"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Prohibido"),
            _ => (StatusCodes.Status500InternalServerError, "Error inesperado")
        };

        var extensions = new Dictionary<string, object?>
        {
            ["errors"] = errors.Select(e => new { e.Code, e.Description }).ToArray()
        };

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: first.Description,
            extensions: extensions);
    }
}