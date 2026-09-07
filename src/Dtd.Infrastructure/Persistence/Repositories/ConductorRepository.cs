using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class ConductorRepository : IConductorRepository
{
    private readonly DtdDbContext _dbContext;

    public ConductorRepository(DtdDbContext dbContext) =>
        _dbContext = dbContext;

    /// <summary>
    /// Devuelve el conductor si existe Y está vinculado a <paramref name="agenciaId"/>
    /// (join <c>conductor_agencias</c>), activo o no
    /// (el caller distingue 404 vs <c>Inactivo</c>).
    /// </summary>
    public async Task<Conductor?> GetByAgenciaYIdAsync(
        Guid agenciaId,
        Guid conductorId,
        CancellationToken cancellationToken = default) =>
        await (
            from ca in _dbContext.ConductorAgencias.AsNoTracking()
            where ca.AgenciaId == agenciaId
               && ca.ConductorId == conductorId
            join c in _dbContext.Conductores.AsNoTracking()
                on ca.ConductorId equals c.Id
            select c
        ).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Conductor>> ListarPorAgenciaAsync(
        Guid agenciaId,
        CancellationToken cancellationToken = default) =>
        await (
            from ca in _dbContext.ConductorAgencias.AsNoTracking()
            where ca.AgenciaId == agenciaId
            join c in _dbContext.Conductores.AsNoTracking()
                on ca.ConductorId equals c.Id
            where c.Activo
            orderby c.Nombre
            select c
        ).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Conductor>> ObtenerConductoresDefectoAsync(
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default)
    {
        // Opcional pero recomendable:
        // valida que almacén y agencia pertenezcan a la empresa indicada.
        var relacionValida =
            await (
                from almacen in _dbContext.Almacenes.AsNoTracking()
                where almacen.Id == almacenId
                   && almacen.Empresa == empresa

                from agencia in _dbContext.Agencias.AsNoTracking()
                where agencia.Id == agenciaId
                   && agencia.Empresa == empresa

                select 1
            ).AnyAsync(cancellationToken);

        if (!relacionValida)
        {
            return Array.Empty<Conductor>();
        }

        var conductorIds = await _dbContext
            .AlmacenAgenciaConductoresDefecto
            .AsNoTracking()
            .Where(d =>
                d.AlmacenId == almacenId &&
                d.AgenciaId == agenciaId)
            .OrderBy(d => d.ConductorId)
            .Select(d => d.ConductorId)
            .ToListAsync(cancellationToken);

        if (conductorIds.Count == 0)
        {
            return Array.Empty<Conductor>();
        }

        return await (
            from ca in _dbContext.ConductorAgencias.AsNoTracking()
            where ca.AgenciaId == agenciaId
               && conductorIds.Contains(ca.ConductorId)

            join c in _dbContext.Conductores.AsNoTracking()
                on ca.ConductorId equals c.Id

            where c.Activo
            orderby c.Nombre
            select c
        ).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Persiste el conductor y sus vínculos iniciales con agencias
    /// (filas de <c>conductor_agencias</c>).
    /// No hace <c>SaveChanges</c>.
    /// </summary>
    public async Task AddAsync(
        Conductor conductor,
        IReadOnlyCollection<Guid> agenciaIds,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Conductores.AddAsync(
            conductor,
            cancellationToken);

        foreach (var agenciaId in agenciaIds.Distinct())
        {
            await _dbContext.ConductorAgencias.AddAsync(
                new ConductorAgencia
                {
                    ConductorId = conductor.Id,
                    AgenciaId = agenciaId
                },
                cancellationToken);
        }
    }
}