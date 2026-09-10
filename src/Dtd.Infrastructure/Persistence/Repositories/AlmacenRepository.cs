using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class AlmacenRepository : IAlmacenRepository
{
    private readonly DtdDbContext _dbContext;

    public AlmacenRepository(DtdDbContext dbContext) =>
        _dbContext = dbContext;

    public Task<Almacen?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _dbContext.Almacenes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);

    public async Task<IReadOnlyList<Almacen>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Almacen>();
        }

        return await _dbContext.Almacenes
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Almacen?> GetByEmpresaYCodigoAsync(
        string empresa,
        string codigo,
        CancellationToken cancellationToken = default) =>
        _dbContext.Almacenes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Empresa == empresa &&
                     a.Codigo == codigo,
                cancellationToken);

    public async Task<IReadOnlyList<Almacen>> ListarPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Almacenes
            .AsNoTracking()
            .Where(a =>
                a.Empresa == empresa &&
                a.Activo)
            .OrderBy(a => a.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Agencia>> ListarAgenciasDisponiblesAsync(
        Guid almacenId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from link in _dbContext.AlmacenAgencias.AsNoTracking()
            join agencia in _dbContext.Agencias.AsNoTracking()
                on link.AgenciaId equals agencia.Id
            where link.AlmacenId == almacenId &&
                  agencia.Activa
            orderby agencia.Nombre
            select agencia
        ).ToListAsync(cancellationToken);
    }

    public async Task<bool> EsAgenciaDisponibleAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from link in _dbContext.AlmacenAgencias.AsNoTracking()
            join agencia in _dbContext.Agencias.AsNoTracking()
                on link.AgenciaId equals agencia.Id
            where link.AlmacenId == almacenId &&
                  link.AgenciaId == agenciaId &&
                  agencia.Activa
            select link
        ).AnyAsync(cancellationToken);
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
                x =>
                    x.AlmacenId == almacenId &&
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
                x =>
                    x.AlmacenId == almacenId &&
                    x.AgenciaId == agenciaId,
                cancellationToken);
    }

    public Task AddAsync(
        Almacen almacen,
        CancellationToken cancellationToken = default) =>
        _dbContext.Almacenes
            .AddAsync(almacen, cancellationToken)
            .AsTask();

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

    public async Task<(IReadOnlyList<Almacen> Items, int Total)> BuscarAsync(
    string empresa,
    IReadOnlyCollection<Guid> idsPermitidos,
    string? texto,
    bool? activo,
    int skip,
    int take,
    CancellationToken cancellationToken = default)
    {
        if (idsPermitidos.Count == 0)
        {
            return (Array.Empty<Almacen>(), 0);
        }

        var query = _dbContext.Almacenes
            .AsNoTracking()
            .Where(a =>
                a.Empresa == empresa &&
                idsPermitidos.Contains(a.Id));

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = $"%{texto.Trim()}%";

            query = query.Where(a =>
                EF.Functions.ILike(a.Codigo, filtro) ||
                EF.Functions.ILike(a.Nombre, filtro) ||
                EF.Functions.ILike(a.Ciudad, filtro));
        }

        if (activo.HasValue)
        {
            query = query.Where(a =>
                a.Activo == activo.Value);
        }

        var total = await query.CountAsync(
            cancellationToken);

        var items = await query
            .OrderBy(a => a.Nombre)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}