namespace Dtd.Application.GatewayContracts;

public sealed record DocutenCallbackLogEntry(
    Guid Id,
    DateTimeOffset RecibidoEn,
    string Tipo,
    Guid? DocumentoId,
    string? LotId,
    string? LotReference,
    string? ShipmentId,
    string? ShipmentReference,
    string? Event,
    string? Estado,
    bool Procesado,
    string Payload,
    string? Headers,
    string? Mensaje);
