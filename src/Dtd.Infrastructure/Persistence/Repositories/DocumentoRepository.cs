using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class DocumentoRepository : IDocumentoRepository
{
    private readonly DtdDbContext _dbContext;

    public DocumentoRepository(DtdDbContext dbContext) => _dbContext = dbContext;
    public Task<DocumentoDigitalTransporte?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default) =>
    _dbContext.Documentos
        .Include(d => d.Expediciones)
        .Include(d => d.Conductores)
        .Include(d => d.Ccs)
        .Include(d => d.Envios)
        .FirstOrDefaultAsync(
            d => d.Id == id,
            cancellationToken);

    public Task AddAsync(DocumentoDigitalTransporte documento, CancellationToken cancellationToken = default) =>
        _dbContext.Documentos.AddAsync(documento, cancellationToken).AsTask();

    public async Task<IReadOnlySet<string>> ObtenerErpIdsIncluidosAsync(
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        IReadOnlyCollection<string> erpIds,
        CancellationToken cancellationToken = default)
    {
        // "no incluidas todavía en ninguno": exclude any expedition already stored for the same
        // company/warehouse/carrier combo (el scope por el que se pidió al ERP). El scope ahora va
        // por los Ids (FK) del almacén/agencia, no por los códigos.
        var incluidos = await _dbContext.Expediciones
            .Where(e => e.Empresa == empresa
                && e.AlmacenId == almacenId
                && e.AgenciaId == agenciaId
                && erpIds.Contains(e.ErpId))
            .Select(e => e.ErpId)
            .ToListAsync(cancellationToken);

        return incluidos.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<DocumentoDigitalTransporte>> ListarAsync(
    DocumentoFiltro filtro,
    CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Documentos
            .Include(d => d.Expediciones)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Empresa))
        {
            query = query.Where(d => d.Empresa == filtro.Empresa);
        }

        if (filtro.Empresas is { Count: > 0 } empresas)
        {
            query = query.Where(d => empresas.Contains(d.Empresa));
        }

        if (filtro.AlmacenId is { } almacenId)
        {
            query = query.Where(d => d.AlmacenId == almacenId);
        }

        if (filtro.AgenciaId is { } agenciaId)
        {
            query = query.Where(d => d.AgenciaId == agenciaId);
        }

        if (filtro.FechaDesde is { } desde)
        {
            query = query.Where(d => d.RangoFechas.FechaDesde >= desde);
        }

        if (filtro.FechaHasta is { } hasta)
        {
            query = query.Where(d => d.RangoFechas.FechaHasta <= hasta);
        }

        if (filtro.Estado is { } estado)
        {
            query = query.Where(d => d.Estado == estado);
        }

        if (filtro.Finalizado is { } finalizado)
        {
            query = query.Where(d => d.Finalizado == finalizado);
        }

        return await query
            .OrderByDescending(d => d.FechaGeneracion)
            .ToListAsync(cancellationToken);
    }
}
