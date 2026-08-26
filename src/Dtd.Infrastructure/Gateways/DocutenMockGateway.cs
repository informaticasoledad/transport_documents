using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;

namespace Dtd.Infrastructure.Gateways;

/// <summary>
/// In-memory Docuten gateway. Simula el contrato real de lotes: al crear devuelve un <c>lot_id</c> en
/// estado <c>pending</c> (lote async); al sondear devuelve el lote <c>completed</c> con un shipment
/// completado (simula que el carrier acepta y el transporte finaliza → <c>Finalizado</c>).
/// </summary>
internal sealed class DocutenMockGateway : IDocutenGateway
{
    public Task<DocutenLoteEnvioResult> EnviarAsync(DocutenLoteDto lote, CancellationToken cancellationToken = default)
    {
        var lotId = $"LOT-{Guid.NewGuid():N}".ToUpperInvariant();
        var shipments = lote.Shipments
            .Select(s => new DocutenShipmentEnvioResult(
                s.ShipmentReference,
                $"SHP-{Guid.NewGuid():N}".ToUpperInvariant(),
                EstadoDocuten.Pending))
            .ToList();

        return Task.FromResult(new DocutenLoteEnvioResult(lotId, EstadoDocuten.Pending, shipments));
    }

    public Task<DocutenLoteEstadoResult> ObtenerEstadoAsync(string lotId, CancellationToken cancellationToken = default) =>
        Task.FromResult(new DocutenLoteEstadoResult(
            EstadoDocuten.Completed,
            [new() { ShipmentStatus = EstadoDocuten.Completed }]));

    // La cancelación por shipment es no-op en el mock: la anulación local se vuelca igual y el
    // siguiente sondeo reflejará el estado que devuelva ObtenerEstadoAsync (Completed en el mock).
    public Task CancelarAsync(string lotId, string reason, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
