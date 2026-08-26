using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class AlmacenRepository : IAlmacenRepository
{
    private readonly DtdDbContext _dbContext;

    public AlmacenRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public Task<Almacen?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Almacenes.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Almacen>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Almacen>();
        }

        return await _dbContext.Almacenes.AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Almacen?> GetByEmpresaYCodigoAsync(string empresa, string codigo, CancellationToken cancellationToken = default) =>
        _dbContext.Almacenes.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Empresa == empresa && a.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Almacen>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default) =>
        await _dbContext.Almacenes.AsNoTracking()
            .Where(a => a.Empresa == empresa && a.Activo)
            .OrderBy(a => a.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Agencia>> ListarAgenciasDisponiblesAsync(
        string empresa, string codigo, CancellationToken cancellationToken = default)
    {
        var almacenId = await ResolveAlmacenIdAsync(empresa, codigo, cancellationToken);
        if (almacenId == Guid.Empty)
        {
            return Array.Empty<Agencia>();
        }

        return await
            (from link in _dbContext.AlmacenAgencias.AsNoTracking()
             join agencia in _dbContext.Agencias.AsNoTracking() on link.AgenciaId equals agencia.Id
             where link.AlmacenId == almacenId && agencia.Empresa == empresa && agencia.Activa
             orderby agencia.Nombre
             select agencia)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EsAgenciaDisponibleAsync(
        Guid almacenId, Guid agenciaId, CancellationToken cancellationToken = default)
    {
        // La agencia está disponible para el almacén si existe la tupla en almacen_agencias y la
        // agencia está activa. El join va por los Ids (FK); no hace falta re-validar empresa porque
        // el almacén y la agencia ya se han validado contra la empresa en el handler.
        return await
            (from link in _dbContext.AlmacenAgencias.AsNoTracking()
             join agencia in _dbContext.Agencias.AsNoTracking() on link.AgenciaId equals agencia.Id
             where link.AlmacenId == almacenId && link.AgenciaId == agenciaId && agencia.Activa
             select link)
            .AnyAsync(cancellationToken);
    }

    public Task<AlmacenAgencia?> GetRelacionAgenciaAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AlmacenAgencias
            .AsNoTracking()
            .Include(x => x.Template)
            .FirstOrDefaultAsync(
                x => x.AlmacenId == almacenId &&
                     x.AgenciaId == agenciaId,
                cancellationToken);
    }

    public Task<AlmacenAgencia?> GetRelacionAgenciaParaActualizarAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AlmacenAgencias
            .FirstOrDefaultAsync(
                x => x.AlmacenId == almacenId && x.AgenciaId == agenciaId,
                cancellationToken);
    }

    public Task AddAsync(Almacen almacen, CancellationToken cancellationToken = default) =>
        _dbContext.Almacenes.AddAsync(almacen, cancellationToken).AsTask();

    private Task<Guid> ResolveAlmacenIdAsync(string empresa, string codigo, CancellationToken cancellationToken) =>
        _dbContext.Almacenes.AsNoTracking()
            .Where(a => a.Empresa == empresa && a.Codigo == codigo)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Almacen>> ObtenerPorCodigosAsync(
    string empresa,
    IReadOnlyCollection<string> codigos,
    CancellationToken cancellationToken)
    {
        if (codigos.Count == 0)
        {
            return [];
        }

        return await _dbContext.Almacenes
            .Where(a =>
                a.Empresa == empresa &&
                codigos.Contains(a.Codigo))
            .ToListAsync(cancellationToken);
    }
}
