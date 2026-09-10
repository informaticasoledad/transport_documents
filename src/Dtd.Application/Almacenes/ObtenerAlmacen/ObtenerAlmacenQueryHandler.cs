using Dtd.Domain.Almacenes;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ObtenerAlmacen;

internal sealed class ObtenerAlmacenQueryHandler
    : IRequestHandler<
        ObtenerAlmacenQuery,
        ErrorOr<AlmacenDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ObtenerAlmacenQueryHandler(
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AlmacenDto>> Handle(
        ObtenerAlmacenQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null ||
            !string.Equals(
                almacen.Empresa,
                empresa,
                StringComparison.OrdinalIgnoreCase))
        {
            return Error.NotFound(
                "Almacen.NoEncontrado",
                $"No existe el almacén '{request.AlmacenId}' para la empresa '{empresa}'.");
        }

        var almacenesPermitidos =
            await _accesoAlmacenService.ObtenerAlmacenesPermitidosAsync(
                empresa,
                cancellationToken);

        if (almacenesPermitidos.IsError)
        {
            return almacenesPermitidos.Errors;
        }

        if (!almacenesPermitidos.Value.Contains(almacen.Id))
        {
            return Error.Forbidden(
                "Almacen.SinAcceso",
                "El usuario no tiene acceso al almacén indicado.");
        }

        return ToDto(almacen);
    }

    private static AlmacenDto ToDto(
        Almacen almacen)
    {
        return new AlmacenDto(
            almacen.Id,
            almacen.Codigo,
            almacen.Nombre,
            almacen.Direccion,
            almacen.CodigoPostal,
            almacen.Ciudad,
            almacen.CodigoPaisIso,
            almacen.Email?.Valor,
            almacen.Telefono);
    }
}