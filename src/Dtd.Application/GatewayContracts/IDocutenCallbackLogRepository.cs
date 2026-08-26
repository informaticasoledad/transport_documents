namespace Dtd.Application.GatewayContracts;

public interface IDocutenCallbackLogRepository
{
    Task AddAsync(DocutenCallbackLogEntry entry, CancellationToken cancellationToken = default);

    Task<int> DeleteOlderThanAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocutenCallbackLogEntry>> ListRecentAsync(
        Guid? documentoId,
        int limit,
        CancellationToken cancellationToken = default);
}
