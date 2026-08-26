namespace Dtd.Domain.Common;

/// <summary>
/// Unit of work port: commits pending changes tracked by the repositories.
/// Implemented in the infrastructure layer (EF Core DbContext).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}