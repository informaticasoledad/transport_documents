using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ModificarAlmacen;

internal sealed class ModificarAlmacenCommandHandler
    : IRequestHandler<
        ModificarAlmacenCommand,
        ErrorOr<AlmacenDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarAlmacenCommandHandler(
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService,
        IUnitOfWork unitOfWork)
    {
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AlmacenDto>> Handle(
        ModificarAlmacenCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdParaActualizarAsync(
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

        almacen.ModificarNombre(request.Nombre);

        almacen.ActualizarDireccion(
            request.Direccion,
            request.CodigoPostal,
            request.Ciudad,
            request.CodigoPaisIso);

        almacen.ActualizarContacto(
            request.Email,
            request.Telefono);

        almacen.ConfigurarTipoFirmaConsignor(
            request.TipoFirmaConsignor);

        if (request.Activo)
        {
            almacen.Activar();
        }
        else
        {
            almacen.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

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