using Dtd.Domain.Almacenes;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAlmacenes;

internal sealed class ListarAlmacenesQueryHandler
    : IRequestHandler<
        ListarAlmacenesQuery,
        ErrorOr<AlmacenesPaginadosDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAlmacenesQueryHandler(
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AlmacenesPaginadosDto>> Handle(
        ListarAlmacenesQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var page = request.Page < 1
            ? 1
            : request.Page;

        var pageSize = request.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var almacenesPermitidos =
            await _accesoAlmacenService.ObtenerAlmacenesPermitidosAsync(
                empresa,
                cancellationToken);

        if (almacenesPermitidos.IsError)
        {
            return almacenesPermitidos.Errors;
        }

        var idsPermitidos = almacenesPermitidos.Value;

        if (idsPermitidos.Count == 0)
        {
            return new AlmacenesPaginadosDto
            {
                Items = [],
                Total = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        var skip = (page - 1) * pageSize;

        var (items, total) =
            await _almacenRepository.BuscarAsync(
                empresa,
                idsPermitidos,
                request.Texto,
                request.Activo,
                skip,
                pageSize,
                cancellationToken);

        return new AlmacenesPaginadosDto
        {
            Items = items
                .Select(ToDto)
                .ToList(),

            Total = total,
            Page = page,
            PageSize = pageSize
        };
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