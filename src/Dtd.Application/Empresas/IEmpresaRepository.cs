using Dtd.Application.GatewayContracts;

public interface IEmpresaRepository
{
    Task<EmpresaConfig?> GetByEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmpresaConfig>> ListarAsync(
        CancellationToken cancellationToken = default);
}