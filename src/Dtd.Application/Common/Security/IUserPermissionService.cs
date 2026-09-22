namespace Dtd.Application.Common.Security;

public interface IUserPermissionService
{
    Task<IReadOnlyCollection<string>> GetAllowedWarehousesAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetAllowedCompaniesAsync(
        CancellationToken cancellationToken = default);
}