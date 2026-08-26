using Dtd.Domain.Documentos;

namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Origen del PDF de cada shipment de Docuten (campo <c>documents[]</c> del lote). La API de Docuten
/// requiere al menos un documento por shipment, así que este puerto lo proporciona.
/// <para><b>Fase 1:</b> implementación <c>PlaceholderDocutenDocumentoProvider</c> que reutiliza un eCMR
/// de prueba en Base64 (mismo PDF que <c>docs/request.json</c>) con signers por party.</para>
/// <para><b>Fase 2:</b> origen real del PDF por definir (recuperar del ERP o generar en casa); el
/// contrato es async (I/O) para no tener que cambiar la firma al cablear el origen real.</para>
/// </summary>
public interface IDocutenDocumentoProvider
{
    /// <summary>Construye el <see cref="DocutenDocumentoDto"/> del shipment de un envío (agrupación de
    /// expediciones). Cada envío del lote Docuten requiere al menos un documento.</summary>
    Task<DocutenDocumentoDto> ObtenerDocumentoAsync(
        DocumentoDigitalTransporte documento,
        Envio envio,
        CancellationToken cancellationToken = default);
}