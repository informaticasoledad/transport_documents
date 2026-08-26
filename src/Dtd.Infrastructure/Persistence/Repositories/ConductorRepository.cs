using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class ConductorRepository : IConductorRepository
{
    private readonly DtdDbContext _dbContext;

    public ConductorRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    /// <summary>Devuelve el conductor si existe Y está vinculado a <paramref name="agenciaId"/>
    /// (join <c>conductor_agencias</c>), activo o no (el caller distingue 404 vs <c>Inactivo</c>).</summary>
    public async Task<Conductor?> GetByAgenciaYIdAsync(Guid agenciaId, Guid conductorId, CancellationToken cancellationToken = default) =>
        await (from ca in _dbContext.ConductorAgencias.AsNoTracking()
               where ca.AgenciaId == agenciaId && ca.ConductorId == conductorId
               join c in _dbContext.Conductores.AsNoTracking() on ca.ConductorId equals c.Id
               select c)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Conductor>> ListarPorAgenciaAsync(Guid agenciaId, CancellationToken cancellationToken = default) =>
        await (from ca in _dbContext.ConductorAgencias.AsNoTracking()
               where ca.AgenciaId == agenciaId
               join c in _dbContext.Conductores.AsNoTracking() on ca.ConductorId equals c.Id
               where c.Activo
               orderby c.Nombre
               select c)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Conductor>> ObtenerConductoresDefectoAsync(
        string empresa, string almacenCodigo, string agenciaCodigo, CancellationToken cancellationToken = default)
    {
        var almacenId = await ResolveAlmacenIdAsync(empresa, almacenCodigo, cancellationToken);
        if (almacenId == Guid.Empty)
        {
            return Array.Empty<Conductor>();
        }

        var agenciaId = await ResolveAgenciaIdAsync(empresa, agenciaCodigo, cancellationToken);
        if (agenciaId == Guid.Empty)
        {
            return Array.Empty<Conductor>();
        }

        // Defaults de la tupla (almacén, agencia) resueltos al catálogo, sólo activos Y vinculados a
        // esa agencia (defense-in-depth: un default que apunta a un conductor desvinculado se excluye).
        var conductorIds = await
            (from d in _dbContext.AlmacenAgenciaConductoresDefecto.AsNoTracking()
             where d.AlmacenId == almacenId && d.AgenciaId == agenciaId
             orderby d.ConductorId
             select d.ConductorId)
            .ToListAsync(cancellationToken);

        if (conductorIds.Count == 0)
        {
            return Array.Empty<Conductor>();
        }

        var vinculados = await
            (from ca in _dbContext.ConductorAgencias.AsNoTracking()
             where ca.AgenciaId == agenciaId && conductorIds.Contains(ca.ConductorId)
             join c in _dbContext.Conductores.AsNoTracking() on ca.ConductorId equals c.Id
             where c.Activo
             orderby c.Nombre
             select c)
            .ToListAsync(cancellationToken);

        return vinculados;
    }

    /// <summary>Persiste el conductor y sus vínculos iniciales con agencias (filas de
    /// <c>conductor_agencias</c>). No hace <c>SaveChanges</c>.</summary>
    public async Task AddAsync(Conductor conductor, IReadOnlyCollection<Guid> agenciaIds, CancellationToken cancellationToken = default)
    {
        await _dbContext.Conductores.AddAsync(conductor, cancellationToken);

        foreach (var agenciaId in agenciaIds.Distinct())
        {
            await _dbContext.ConductorAgencias.AddAsync(
                new ConductorAgencia { ConductorId = conductor.Id, AgenciaId = agenciaId }, cancellationToken);
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