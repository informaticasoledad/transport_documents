namespace Dtd.Domain.Empresas;

public interface IEmpresaRepository
{
    Task<IReadOnlyList<Empresa>> ListarActivasAsync(
        CancellationToken cancellationToken = default);

    Task<Empresa?> GetByCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default);
}