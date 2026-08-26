using Dtd.Domain.Almacenes;
using Dtd.Domain.AgenciaBases;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class AgenciaBaseRepository : IAgenciaBaseRepository
{
    private readonly DtdDbContext _dbContext;

    public AgenciaBaseRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task<AgenciaBase?> GetByIdAsync(
        Guid agenciaBaseId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AgenciaBases
            .FirstOrDefaultAsync(x => x.Id == agenciaBaseId, cancellationToken);
    }

    public async Task<AgenciaBase?> GetByEmpresaYCodigoAsync(
        string empresa,
        string codigo,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AgenciaBases.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Empresa == empresa && c.Codigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<AgenciaBase>> ListarPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AgenciaBases.AsNoTracking()
            .Where(c => c.Empresa == empresa)
            .OrderBy(c => c.Codigo)
            .ThenBy(c => c.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgenciaBase>> ListarActivosPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AgenciaBases.AsNoTracking()
            .Where(c => c.Empresa == empresa && c.Activo)
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Codigo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgenciaBase>> ObtenerAgenciaBasesDefectoAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        CancellationToken cancellationToken = default)
    {
        var almacenId = await ResolveAlmacenIdAsync(empresa, almacenCodigo, cancellationToken);
        if (almacenId == Guid.Empty)
        {
            return Array.Empty<AgenciaBase>();
        }

        var agenciaId = await ResolveAgenciaIdAsync(empresa, agenciaCodigo, cancellationToken);
        if (agenciaId == Guid.Empty)
        {
            return Array.Empty<AgenciaBase>();
        }

        var agenciaBaseIds = await
            (from d in _dbContext.AlmacenAgenciaBasesDefecto.AsNoTracking()
             where d.AlmacenId == almacenId && d.AgenciaId == agenciaId
             orderby d.AgenciaBaseId
             select d.AgenciaBaseId)
            .ToListAsync(cancellationToken);

        if (agenciaBaseIds.Count == 0)
        {
            return Array.Empty<AgenciaBase>();
        }

        return await
            (from c in _dbContext.AgenciaBases.AsNoTracking()
             where agenciaBaseIds.Contains(c.Id) && c.Activo
             where c.Empresa == empresa
             orderby c.Nombre
             select c)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        AgenciaBase agenciaBase,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AgenciaBases.AddAsync(agenciaBase, cancellationToken);
    }

    public Task ActualizarAsync(
        AgenciaBase agenciaBase,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task SetDefectosAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        IReadOnlyCollection<Guid> agenciaBaseIds,
        CancellationToken cancellationToken = default)
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

        var actuales = await _dbContext.AlmacenAgenciaBasesDefecto
            .Where(x => x.AlmacenId == almacenId && x.AgenciaId == agenciaId)
            .ToListAsync(cancellationToken);
        if (actuales.Count > 0)
        {
            _dbContext.AlmacenAgenciaBasesDefecto.RemoveRange(actuales);
        }

        foreach (var agenciaBaseId in agenciaBaseIds.Where(id => id != Guid.Empty).Distinct())
        {
            await _dbContext.AlmacenAgenciaBasesDefecto.AddAsync(
                AlmacenAgenciaBaseDefecto.Crear(almacenId, agenciaId, agenciaBaseId),
                cancellationToken);
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
}
