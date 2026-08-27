using Dtd.Application.Almacenes;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAgenciasPorAlmacen;

/// <summary>
/// Lista las agencias (carriers) disponibles para un almacén de una empresa
/// (unión <c>almacen_agencias</c>), para el dropdown de selección del front.
/// Los conductores por defecto de cada tupla se consultan por separado
/// vía endpoint dedicado.
/// </summary>
public sealed record ListarAgenciasPorAlmacenQuery(
    string Empresa,
    string AlmacenCodigo)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaDto>>>;

internal sealed class ListarAgenciasPorAlmacenQueryHandler
    : IRequestHandler<
        ListarAgenciasPorAlmacenQuery,
        ErrorOr<IReadOnlyList<AgenciaDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAgenciasPorAlmacenQueryHandler(
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaDto>>> Handle(
        ListarAgenciasPorAlmacenQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen =
            await _almacenRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.AlmacenCodigo,
                cancellationToken);

        if (almacen is null)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenCodigo}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                empresa,
                almacen.Id,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        var agencias =
            await _almacenRepository.ListarAgenciasDisponiblesAsync(
                empresa,
                request.AlmacenCodigo,
                cancellationToken);

        return agencias
            .Select(a => new AgenciaDto(
                a.Id,
                a.Codigo,
                a.Nombre))
            .ToList();
    }
}