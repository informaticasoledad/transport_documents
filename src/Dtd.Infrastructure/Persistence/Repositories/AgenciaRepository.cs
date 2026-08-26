using Dtd.Domain.Agencias;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class AgenciaRepository : IAgenciaRepository
{
    private readonly DtdDbContext _dbContext;

    public AgenciaRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public Task<Agencia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Agencias.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Agencia>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Agencia>();
        }

        return await _dbContext.Agencias.AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Agencia?> GetByEmpresaYCodigoAsync(string empresa, string codigo, CancellationToken cancellationToken = default) =>
        _dbContext.Agencias.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Empresa == empresa && a.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Agencia>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default) =>
        await _dbContext.Agencias.AsNoTracking()
            .Where(a => a.Empresa == empresa && a.Activa)
            .OrderBy(a => a.Nombre)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Agencia agencia, CancellationToken cancellationToken = default) =>
        _dbContext.Agencias.AddAsync(agencia, cancellationToken).AsTask();
}