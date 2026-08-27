using ErrorOr;

namespace Dtd.Application.Security;

public interface IContextoAccesoService
{
    Task<ErrorOr<ContextoAcceso>> ObtenerAsync(
        string empresa,
        CancellationToken cancellationToken = default);
}