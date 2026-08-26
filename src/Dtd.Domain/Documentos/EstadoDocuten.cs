namespace Dtd.Domain.Documentos;

/// <summary>
/// Status que reporta Docuten para un shipment de un lote (campo <c>shipment_status</c> del contrato real
/// <c>/api/v1/lots</c>). Modelado como string-backed enum para que valores desconocidos del sandbox no
/// rompan la deserialización. Docuten no tiene estado "rechazado": una negativa a firmar se refleja como
/// <c>error</c> o <c>cancelled</c>. El estado <c>Anulado</c> de nuestro pipeline es local (forzado desde
/// el front), no un estado de Docuten.
/// </summary>
public static class EstadoDocuten
{
    public const string Success = "success";
    public const string Pending = "pending";
    public const string Created = "created";
    public const string ReadyForPickup = "ready_for_pickup";
    public const string PendingDelivery = "pending_delivery";
    public const string Delivered = "delivered";
    public const string Completed = "completed";
    public const string Error = "error";
    public const string Cancelled = "cancelled";
}
