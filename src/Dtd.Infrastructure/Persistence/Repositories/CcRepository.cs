using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class CcRepository : ICcRepository
{
    private readonly DtdDbContext _dbContext;

    public CcRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task<Cc?> GetByIdAsync(Guid ccId, CancellationToken cancellationToken = default) =>
        await _dbContext.Ccs
            .FirstOrDefaultAsync(c => c.Id == ccId, cancellationToken);

    public async Task<Cc?> GetByEmpresaYCodigoAsync(string empresa, string codigo, CancellationToken cancellationToken = default) =>
        await _dbContext.Ccs.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Empresa == empresa && c.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Cc>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default) =>
        await _dbContext.Ccs.AsNoTracking()
            .Where(c => c.Empresa == empresa)
            .OrderBy(c => c.Codigo)
            .ThenBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Cc>> ListarPorAlmacenAsync(Guid almacenId, CancellationToken cancellationToken = default) =>
        await (from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
              where relacion.AlmacenId == almacenId
              join c in _dbContext.Ccs.AsNoTracking() on relacion.CcId equals c.Id
              where c.Activo
              orderby c.Nombre
              select c)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Cc>> ListarPorAgenciaAsync(Guid agenciaId, CancellationToken cancellationToken = default) =>
        await (from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
              where relacion.AgenciaId == agenciaId
              join c in _dbContext.Ccs.AsNoTracking() on relacion.CcId equals c.Id
              where c.Activo
              orderby c.Nombre
              select c)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<Cc?> GetByAlmacenYAgenciaEIdAsync(
        Guid almacenId, Guid agenciaId, Guid ccId, CancellationToken cancellationToken = default)
    {
        var disponible = await _dbContext.AlmacenAgenciaCcs.AsNoTracking()
            .AnyAsync(
                x => x.AlmacenId == almacenId &&
                     x.AgenciaId == agenciaId &&
                     x.CcId == ccId,
                cancellationToken);

        if (!disponible)
        {
            return null;
        }

        return await _dbContext.Ccs.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == ccId, cancellationToken);
    }

    public async Task<IReadOnlyList<Cc>> ObtenerCcsDefectoAsync(
        string empresa, string almacenCodigo, string agenciaCodigo, CancellationToken cancellationToken = default)
    {
        var almacenId = await ResolveAlmacenIdAsync(empresa, almacenCodigo, cancellationToken);
        if (almacenId == Guid.Empty)
        {
            return Array.Empty<Cc>();
        }

        var agenciaId = await ResolveAgenciaIdAsync(empresa, agenciaCodigo, cancellationToken);
        if (agenciaId == Guid.Empty)
        {
            return Array.Empty<Cc>();
        }

        return await
            (from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
             where relacion.AlmacenId == almacenId &&
                   relacion.AgenciaId == agenciaId &&
                   relacion.PorDefecto
             join c in _dbContext.Ccs.AsNoTracking() on relacion.CcId equals c.Id
             where c.Activo
             orderby c.Nombre
             select c)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Cc cc,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Ccs.AddAsync(cc, cancellationToken);

        foreach (var vinculo in Deduplicar(vinculos))
        {
            await _dbContext.AlmacenAgenciaCcs.AddAsync(
                AlmacenAgenciaCc.Crear(
                    vinculo.AlmacenId,
                    vinculo.AgenciaId,
                    cc.Id,
                    vinculo.PorDefecto),
                cancellationToken);
        }
    }

    public async Task ActualizarAsync(
        Cc cc,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken = default)
    {
        var actuales = await _dbContext.AlmacenAgenciaCcs
            .Where(x => x.CcId == cc.Id)
            .ToListAsync(cancellationToken);

        if (actuales.Count > 0)
        {
            _dbContext.AlmacenAgenciaCcs.RemoveRange(actuales);
        }

        foreach (var vinculo in Deduplicar(vinculos))
        {
            await _dbContext.AlmacenAgenciaCcs.AddAsync(
                AlmacenAgenciaCc.Crear(
                    vinculo.AlmacenId,
                    vinculo.AgenciaId,
                    cc.Id,
                    vinculo.PorDefecto),
                cancellationToken);
        }
    }

    public async Task SetDefectosAsync(
        string empresa, string almacenCodigo, string agenciaCodigo,
        IReadOnlyCollection<Guid> ccIds, CancellationToken cancellationToken = default)
    {
        var almacenId = await ResolveAlmacenIdAsync(empresa, almacenCodigo, cancellationToken);
        if (almacenId == Guid.Empty)
        {
            return;
        }

        var agenciaId = await ResolveAgenciaIdAsync(empresa, agenciaCodigo, cancellationToken);
        if (agenciaId == Guid.Empty)
        {
            return;
        }

        var idsDefecto = ccIds.Distinct().ToHashSet();
        var relaciones = await _dbContext.AlmacenAgenciaCcs
            .Where(x => x.AlmacenId == almacenId && x.AgenciaId == agenciaId)
            .ToListAsync(cancellationToken);

        foreach (var relacion in relaciones)
        {
            relacion.ConfigurarPorDefecto(idsDefecto.Contains(relacion.CcId));
        }
    }

    private Task<Guid> ResolveAlmacenIdAsync(string empresa, string codigo, CancellationToken cancellationToken) =>
        _dbContext.Almacenes.AsNoTracking()
            .Where(a => a.Empresa == empresa && a.Codigo == codigo)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

    private Task<Guid> ResolveAgenciaIdAsync(string empresa, string codigo, CancellationToken cancellationToken) =>
        _dbContext.Agencias.AsNoTracking()
            .Where(a => a.Empresa == empresa && a.Codigo == codigo)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

    private static IReadOnlyList<CcVinculoAlmacenAgencia> Deduplicar(
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos) =>
        vinculos
            .GroupBy(x => new { x.AlmacenId, x.AgenciaId })
            .Select(g => new CcVinculoAlmacenAgencia(
                g.Key.AlmacenId,
                g.Key.AgenciaId,
                g.Any(x => x.PorDefecto)))
            .ToList();
}
