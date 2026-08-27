using ErrorOr;

namespace Dtd.Application.Almacenes;

public interface IAccesoAlmacenService
{
    Task<ErrorOr<Success>> ValidarAccesoAsync(
        string empresa,
        Guid almacenId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<Success>> ValidarAccesoEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<IReadOnlyCollection<Guid>>> ObtenerAlmacenesPermitidosAsync(
        string empresa,
        CancellationToken cancellationToken = default);
}