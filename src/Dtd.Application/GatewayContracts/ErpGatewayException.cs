namespace Dtd.Application.GatewayContracts;

/// <summary>
/// La lanza el gateway del ERP cuando la llamada HTTP devuelve un status de error (4xx/5xx).
/// Lleva el código de estado, el reason phrase, el **cuerpo** y las **cabeceras** de la respuesta
/// del ERP (truncados) para que el handler pueda elevarlo en el <c>Error</c> y quede visible para
/// diagnosticar el fallo (p.ej. un 403 con cuerpo "Forbidden" cuyas cabeceras —Server,
/// WWW-Authenticate, Via— revelan quién lo deniega). El <c>Authorization</c> nunca se incluye aquí.
/// </summary>
public sealed class ErpGatewayException : Exception
{
    /// <summary>Código de estado HTTP devuelto por el ERP.</summary>
    public int StatusCode { get; }

    /// <summary>Reason phrase de la respuesta del ERP (puede estar vacío).</summary>
    public string ReasonPhrase { get; }

    /// <summary>Cuerpo de la respuesta del ERP, truncado a un tamaño seguro para propagar.</summary>
    public string Body { get; }

    /// <summary>Cabeceras de la respuesta del ERP en forma compacta ("k: v; k: v"), truncadas.</summary>
    public string ResponseHeaders { get; }

    public ErpGatewayException(int statusCode, string? reasonPhrase, string body, string responseHeaders)
        : base($"El ERP respondió {statusCode}{(string.IsNullOrWhiteSpace(reasonPhrase) ? null : $" {reasonPhrase}")}." +
               (string.IsNullOrWhiteSpace(body) ? string.Empty : $" Cuerpo: {body}") +
               (string.IsNullOrWhiteSpace(responseHeaders) ? string.Empty : $" Cabeceras: {responseHeaders}"))
    {
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase ?? string.Empty;
        Body = body;
        ResponseHeaders = responseHeaders ?? string.Empty;
    }
}