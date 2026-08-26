namespace Dtd.Infrastructure.Persistence.IntegrationLogs;

internal sealed class DocutenCallbackLog
{
    public Guid Id { get; set; }
    public DateTimeOffset RecibidoEn { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid? DocumentoId { get; set; }
    public string? LotId { get; set; }
    public string? LotReference { get; set; }
    public string? ShipmentId { get; set; }
    public string? ShipmentReference { get; set; }
    public string? Event { get; set; }
    public string? Estado { get; set; }
    public bool Procesado { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string? Headers { get; set; }
    public string? Mensaje { get; set; }
}
