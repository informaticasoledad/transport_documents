using System.Net;
using System.Net.Http;

namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Excepción del gateway HTTP de Docuten. Deriva de <see cref="HttpRequestException"/> para que los
/// handlers que ya atrapan <c>HttpRequestException</c> sigan obteniendo el <see cref="HttpRequestException.StatusCode"/>
/// (y así registrarlo en el intento fallido), mientras el <see cref="Exception.Message"/> incluye el
/// cuerpo de la respuesta (típicamente errores de validación del sandbox: campos obligatorios, etc.).
/// </summary>
public sealed class DocutenGatewayException : HttpRequestException
{
    public new int StatusCode { get; }
    public string Body { get; }

    public DocutenGatewayException(int statusCode, string body)
        : base($"Docuten respondió {statusCode}. Cuerpo: {body}", inner: null, (HttpStatusCode)statusCode)
    {
        StatusCode = statusCode;
        Body = body;
    }
}