using ErrorOr;

namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Convierte una <see cref="ErpGatewayException"/> (respuesta de error del ERP) en un <see cref="Error"/>
/// tipado según el status del ERP, incluyendo el cuerpo en la descripción para que se pueda diagnosticar
/// el motivo real (visible en el ProblemDetails y en log). 401 → Unauthorized, 403 → Forbidden,
/// 404 → NotFound, resto → Failure.
/// </summary>
public static class ErpGatewayErrorMapper
{
    public static Error ToError(ErpGatewayException ex)
    {
        var detail = $"El ERP respondió {ex.StatusCode}" +
                     (string.IsNullOrWhiteSpace(ex.ReasonPhrase) ? null : $" {ex.ReasonPhrase}") +
                     (string.IsNullOrWhiteSpace(ex.Body) ? null : $". Cuerpo: {ex.Body}") +
                     (string.IsNullOrWhiteSpace(ex.ResponseHeaders) ? null : $". Cabeceras: {ex.ResponseHeaders}");

        return ex.StatusCode switch
        {
            401 => Error.Unauthorized("Documento.ErpTokenInvalido", detail),
            403 => Error.Forbidden("Documento.ErpProhibido", detail),
            404 => Error.NotFound("Documento.ErpNoEncontrado", detail),
            _ => Error.Failure("Documento.ErpError", detail)
        };
    }
}