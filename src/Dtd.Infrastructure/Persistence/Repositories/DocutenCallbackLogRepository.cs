using Dtd.Application.GatewayContracts;
using Dtd.Infrastructure.Persistence.IntegrationLogs;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class DocutenCallbackLogRepository : IDocutenCallbackLogRepository
{
    private readonly DtdDbContext _dbContext;

    public DocutenCallbackLogRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(DocutenCallbackLogEntry entry, CancellationToken cancellationToken = default)
    {
        var log = new DocutenCallbackLog
        {
            Id = entry.Id,
            RecibidoEn = entry.RecibidoEn,
            Tipo = entry.Tipo,
            DocumentoId = entry.DocumentoId,
            LotId = entry.LotId,
            LotReference = entry.LotReference,
            ShipmentId = entry.ShipmentId,
            ShipmentReference = entry.ShipmentReference,
            Event = entry.Event,
            Estado = entry.Estado,
            Procesado = entry.Procesado,
            Payload = entry.Payload,
            Headers = entry.Headers,
            Mensaje = entry.Mensaje
        };

        await _dbContext.DocutenCallbackLogs.AddAsync(log, cancellationToken);
    }

    public Task<int> DeleteOlderThanAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default) =>
        _dbContext.DocutenCallbackLogs
            .Where(x => x.RecibidoEn < threshold)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task<IReadOnlyList<DocutenCallbackLogEntry>> ListRecentAsync(
        Guid? documentoId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DocutenCallbackLogs.AsNoTracking();

        if (documentoId is { } id)
        {
            query = query.Where(x => x.DocumentoId == id);
        }

        return await query
            .OrderByDescending(x => x.RecibidoEn)
            .Take(limit)
            .Select(x => new DocutenCallbackLogEntry(
                x.Id,
                x.RecibidoEn,
                x.Tipo,
                x.DocumentoId,
                x.LotId,
                x.LotReference,
                x.ShipmentId,
                x.ShipmentReference,
                x.Event,
                x.Estado,
                x.Procesado,
                x.Payload,
                x.Headers,
                x.Mensaje))
            .ToListAsync(cancellationToken);
    }
}
