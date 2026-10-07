using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories
{
    internal sealed class CcRepository : ICcRepository
    {
        private readonly DtdDbContext _dbContext;

        public CcRepository(DtdDbContext dbContext) =>
            _dbContext = dbContext;

        public async Task<Cc?> GetByIdAsync(
            Guid ccId,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Ccs
                .FirstOrDefaultAsync(
                    c => c.Id == ccId,
                    cancellationToken);

        public async Task<Cc?> GetByEmpresaYCodigoAsync(
            string empresa,
            string codigo,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Ccs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Empresa == empresa &&
                         c.Codigo == codigo,
                    cancellationToken);

        public async Task<IReadOnlyList<Cc>> ListarPorAlmacenYAgenciaAsync(
           Guid almacenId,
           Guid agenciaId,
           CancellationToken cancellationToken = default) =>
           await (
               from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
               where relacion.AlmacenId == almacenId &&
                     relacion.AgenciaId == agenciaId

               join c in _dbContext.Ccs.AsNoTracking()
                   on relacion.CcId equals c.Id

               where c.Activo
               orderby c.Nombre
               select c
           )
           .Distinct()
           .ToListAsync(cancellationToken);


        public async Task<IReadOnlyList<Cc>> ListarPorEmpresaAsync(
            string empresa,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Ccs
                .AsNoTracking()
                .Where(c => c.Empresa == empresa)
                .OrderBy(c => c.Codigo)
                .ThenBy(c => c.Nombre)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<Cc>> ListarPorAlmacenAsync(
            Guid almacenId,
            CancellationToken cancellationToken = default) =>
            await (
                from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
                where relacion.AlmacenId == almacenId

                join c in _dbContext.Ccs.AsNoTracking()
                    on relacion.CcId equals c.Id

                where c.Activo
                orderby c.Nombre
                select c
            )
            .Distinct()
            .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<Cc>> ListarPorAgenciaAsync(
            Guid agenciaId,
            CancellationToken cancellationToken = default) =>
            await (
                from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
                where relacion.AgenciaId == agenciaId

                join c in _dbContext.Ccs.AsNoTracking()
                    on relacion.CcId equals c.Id

                where c.Activo
                orderby c.Nombre
                select c
            )
            .Distinct()
            .ToListAsync(cancellationToken);

        public async Task<Cc?> GetByAlmacenYAgenciaEIdAsync(
            Guid almacenId,
            Guid agenciaId,
            Guid ccId,
            CancellationToken cancellationToken = default)
        {
            var disponible =
                await _dbContext.AlmacenAgenciaCcs
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.AlmacenId == almacenId &&
                            x.AgenciaId == agenciaId &&
                            x.CcId == ccId,
                        cancellationToken);

            if (!disponible)
            {
                return null;
            }

            return await _dbContext.Ccs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == ccId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Cc>> ObtenerCcsDefectoAsync(
            Guid almacenId,
            Guid agenciaId,
            CancellationToken cancellationToken = default)
        {
            return await (
                from relacion in _dbContext.AlmacenAgenciaCcs.AsNoTracking()
                where relacion.AlmacenId == almacenId &&
                      relacion.AgenciaId == agenciaId &&
                      relacion.PorDefecto

                join c in _dbContext.Ccs.AsNoTracking()
                    on relacion.CcId equals c.Id

                where c.Activo
                orderby c.Nombre
                select c
            ).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Cc cc,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Ccs.AddAsync(
                cc,
                cancellationToken);

        }


        public async Task SetDefectosAsync(
            Guid almacenId,
            Guid agenciaId,
            IReadOnlyCollection<Guid> ccIds,
            CancellationToken cancellationToken = default)
        {
            var idsDefecto = ccIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToHashSet();

            var relaciones =
                await _dbContext.AlmacenAgenciaCcs
                    .Where(x =>
                        x.AlmacenId == almacenId &&
                        x.AgenciaId == agenciaId)
                    .ToListAsync(cancellationToken);

            foreach (var relacion in relaciones)
            {
                relacion.ConfigurarPorDefecto(
                    idsDefecto.Contains(relacion.CcId));
            }
        }
        /*
         *  ojazo borrar
        private static IReadOnlyList<CcVinculoAlmacenAgencia> Deduplicar(
            IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos) =>
            vinculos
                .GroupBy(x => new
                {
                    x.AlmacenId,
                    x.AgenciaId
                })
                .Select(g =>
                    new CcVinculoAlmacenAgencia(
                        g.Key.AlmacenId,
                        g.Key.AgenciaId,
                        g.Any(x => x.PorDefecto)))
                .ToList();*/

        public async Task AgregarDefectoAsync(
            Guid almacenId,
            Guid agenciaId,
            Guid ccId,
            CancellationToken cancellationToken = default)
        {
            var relacion = await _dbContext.AlmacenAgenciaCcs
                .SingleAsync(
                    x =>
                        x.AlmacenId == almacenId &&
                        x.AgenciaId == agenciaId &&
                        x.CcId == ccId,
                    cancellationToken);

            relacion.ConfigurarPorDefecto(true);
        }


        public async Task EliminarDefectoAsync(
    Guid almacenId,
    Guid agenciaId,
    Guid ccId,
    CancellationToken cancellationToken = default)
        {
            var relacion = await _dbContext.AlmacenAgenciaCcs
                .SingleAsync(
                    x =>
                        x.AlmacenId == almacenId &&
                        x.AgenciaId == agenciaId &&
                        x.CcId == ccId,
                    cancellationToken);

            relacion.ConfigurarPorDefecto(false);
        }


        public async Task AgregarVinculoAsync(
    Guid ccId,
    Guid almacenId,
    Guid agenciaId,
    bool porDefecto,
    CancellationToken cancellationToken = default)
        {
            await _dbContext.AlmacenAgenciaCcs.AddAsync(
                AlmacenAgenciaCc.Crear(
                    almacenId,
                    agenciaId,
                    ccId,
                    porDefecto),
                cancellationToken);
        }

        public async Task EliminarVinculoAsync(
        Guid ccId,
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default)
        {
            var relacion = await _dbContext.AlmacenAgenciaCcs
                .FirstOrDefaultAsync(
                    x =>
                        x.CcId == ccId &&
                        x.AlmacenId == almacenId &&
                        x.AgenciaId == agenciaId,
                    cancellationToken);

            if (relacion is not null)
            {
                _dbContext.AlmacenAgenciaCcs.Remove(relacion);
            }
        }



        public async Task<(IReadOnlyList<Cc> Items, int Total)> BuscarAsync(
        string empresa,
        string? texto,
        bool? activo,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Ccs
                .AsNoTracking()
                .Where(c => c.Empresa == empresa);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var filtro = texto.Trim();

                query = query.Where(c =>
                    c.Codigo.Contains(filtro) ||
                    c.Nombre.Contains(filtro));
            }

            if (activo.HasValue)
            {
                query = query.Where(c =>
                    c.Activo == activo.Value);
            }

            var total = await query.CountAsync(
                cancellationToken);

            var items = await query
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Codigo)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, total);
        }
    }
}