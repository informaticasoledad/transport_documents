using Dtd.Application.Almacenes;
using Dtd.Domain.Almacenes;
using Dtd.Domain.AgenciaBases;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases.ListarAgenciaBasesPorAlmacen;

/// <summary>
/// Lista los agencia-bases activos del catálogo vinculados a un almacén de una empresa,
/// para el dropdown de selección del front.
/// El almacén se resuelve por (empresa, almacenCodigo).
/// </summary>
public sealed record ListarAgenciaBasesPorAlmacenQuery(
    string Empresa,
    string AlmacenCodigo)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class ListarAgenciaBasesPorAlmacenQueryHandler
    : IRequestHandler<
        ListarAgenciaBasesPorAlmacenQuery,
        ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAgenciaBasesPorAlmacenQueryHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        ListarAgenciaBasesPorAlmacenQuery request,
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

        var agenciaBases =
            await _agenciaBaseRepository.ListarActivosPorEmpresaAsync(
                empresa,
                cancellationToken);

        return agenciaBases
            .Select(CrearAgenciaBaseCommandHandler.ToDto)
            .ToList();
    }
}