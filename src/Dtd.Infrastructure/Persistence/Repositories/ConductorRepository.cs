using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class ConductorRepository : IConductorRepository
{
    private readonly DtdDbContext _dbContext;

    public ConductorRepository(DtdDbContext dbContext) =>
        _dbContext = dbContext;

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
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default)
    {
        var relacionValida = await _dbContext.AlmacenAgencias
            .AsNoTracking()
            .AnyAsync(
                x => x.AlmacenId == almacenId &&
                     x.AgenciaId == agenciaId,
                cancellationToken);

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

    public async Task AddAsync(
        Conductor conductor,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Conductores.AddAsync(
            conductor,
            cancellationToken);
    }
}