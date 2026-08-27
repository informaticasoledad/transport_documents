using Dtd.Application.Security;
using ErrorOr;

namespace Dtd.Application.Almacenes;

internal sealed class AccesoAlmacenService : IAccesoAlmacenService
{
    private readonly IContextoAccesoService _contextoAccesoService;

    public AccesoAlmacenService(
        IContextoAccesoService contextoAccesoService)
    {
        _contextoAccesoService = contextoAccesoService;
    }

    public async Task<ErrorOr<Success>> ValidarAccesoAsync(
        string empresa,
        Guid almacenId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await _contextoAccesoService.ObtenerAsync(
            empresa,
            cancellationToken);

        if (contexto.IsError)
        {
            return contexto.Errors;
        }

        if (!contexto.Value.AlmacenesIds.Contains(almacenId))
        {
            return Error.Forbidden(
                "Almacen.NoAutorizado",
                "El usuario no tiene acceso al almacén indicado.");
        }

        return Result.Success;
    }

    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> ObtenerAlmacenesPermitidosAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        var contexto = await _contextoAccesoService.ObtenerAsync(
            empresa,
            cancellationToken);

        if (contexto.IsError)
        {
            return contexto.Errors;
        }

        return contexto.Value.AlmacenesIds.ToErrorOr();
    }

    public async Task<ErrorOr<Success>> ValidarAccesoEmpresaAsync(
    string empresa,
    CancellationToken cancellationToken = default)
    {
        var contexto = await _contextoAccesoService.ObtenerAsync(
            empresa,
            cancellationToken);

        if (contexto.IsError)
        {
            return contexto.Errors;
        }

        return Result.Success;
    }
}