namespace Dtd.Application.GatewayContracts;

public interface IDocutenCallbackLogRepository
{
    Task AddAsync(
        DocutenCallbackLogEntry entry,
        CancellationToken cancellationToken = default);

    Task<int> DeleteOlderThanAsync(
        DateTimeOffset threshold,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocutenCallbackLogEntry>> ListRecentAsync(
        Guid? documentoId,
        int limit,
        CancellationToken cancellationToken = default);

    // NUEVO: recuperar un callback por su identificador.
    Task<DocutenCallbackLogEntry?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // NUEVO: actualizar el resultado del reprocesamiento.
    Task ActualizarResultadoAsync(
        Guid id,
        Guid? documentoId,
        bool procesado,
        string? mensaje,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocutenCallbackLogEntry>> ListarPendientesAsync(
    int limit,
    CancellationToken cancellationToken = default);

}