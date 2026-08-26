using Dtd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dtd.Api.HealthChecks;

/// <summary>Readiness health check that verifies the PostgreSQL database is reachable.</summary>
internal sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly DtdDbContext _dbContext;

    public DatabaseHealthCheck(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("PostgreSQL accesible")
                : HealthCheckResult.Unhealthy("No se puede conectar a PostgreSQL");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error conectando a PostgreSQL", ex);
        }
    }
}