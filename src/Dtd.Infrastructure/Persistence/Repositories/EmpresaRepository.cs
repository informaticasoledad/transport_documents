using Dtd.Application.GatewayContracts;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class EmpresaRepository : IEmpresaRepository
{
    private readonly DtdDbContext _dbContext;

    public EmpresaRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task<EmpresaConfig?> GetByEmpresaAsync(string empresa, CancellationToken cancellationToken = default)
    {
        // Proyección EF totalmente traducible a SQL. La tabla `empresas` solo guarda lo que varía por
        // empresa: su base_address. El resto del cliente OAuth2 (token_endpoint, client_id, scope,
        // client_secret) es común a todas y va en appsettings (ErpOptions); el client_secret además se
        // descifra a nivel app (Erp:ClientSecret_Enc → ErpOptions.ClientSecret).
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
}
