using Dtd.Application.GatewayContracts;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class EmpresaRepository : IEmpresaRepository
{
    private readonly DtdDbContext _dbContext;

    public EmpresaRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task<EmpresaConfig?> GetByEmpresaAsync(string empresa, CancellationToken cancellationToken = default)
    {
        
        var row = await _dbContext.Empresas.AsNoTracking()
            .Where(e => e.Id == empresa)
            .Select(e => new
            {
                Codigo = e.Id,
                e.BaseAddress,
                e.TaxId,
                e.Nombre
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new EmpresaConfig(row.Codigo, row.BaseAddress, row.TaxId, row.Nombre);
    }

    public async Task<IReadOnlyList<EmpresaConfig>> ListarAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Empresas
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .Select(e => new EmpresaConfig(
                e.Id,
                e.BaseAddress,
                e.TaxId,
                e.Nombre))
            .ToListAsync(cancellationToken);
    }
}
