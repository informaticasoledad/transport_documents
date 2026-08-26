using System.ComponentModel.DataAnnotations;

namespace Dtd.Infrastructure.Configuration;

/// <summary>
/// Options for the Docuten gestor documental gateway. <see cref="UseMock"/> selects the in-memory
/// implementation while awaiting the sandbox credentials.
/// </summary>
public sealed class DocutenOptions
{
    public bool UseMock { get; set; } = true;

    [Required]
    public string BaseAddress { get; set; } = "http://docuten-sandbox.local/";

    /// <summary>
    /// API key del sandbox/producción de Docuten (la que viaja en la cabecera <c>X-API-KEY</c>).
    /// Es un **secret env-only**: **nunca** se commitea. Se inyecta por <c>Docuten:TokenId</c>
    /// (user-secrets en dev / env var <c>Docuten__TokenId</c> desde un Secret de k8s en prod).
    /// Obligatorio cuando <see cref="UseMock"/> = false (se valida al arrancar); con el mock no hace
    /// falta.
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Cuando es <c>true</c>, el gateway HTTP loguea (Information) el body **completo** de la petición
    /// (lote/shipments/parties) y de la respuesta de Docuten para <c>POST /api/v1/lots</c> y
    /// <c>GET /api/v1/lots/{id}</c>. Es un **toggle de diagnóstico**: el body incluye PII (consignor,
    /// drivers: name/tax_id/móvil/email, dirección del almacén), así que **por defecto va apagado** y se
    /// enciende sólo en dev/sandbox para inspeccionar qué enviamos y qué responde Docuten (incluidos
    /// campos que el DTO de respuesta no deserializa — p.ej. avisos/errores de validación en un 2xx).
    /// Env var <c>Docuten__LogRawPayload=true</c> o <c>Docuten:LogRawPayload</c> en appsettings.
    /// </summary>
    public bool LogRawPayload { get; set; } = false;
}