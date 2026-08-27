using Dtd.Application.Ccs;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarCcsDefecto;

/// <summary>
/// Lista los CCs por defecto de una tupla (empresa, almacén, agencia) para que el front los
/// auto-adjunte al generar un documento para esa tupla. El back no los auto-adjunta — los añade
/// el front vía <c>POST /documentos/{id}/ccs</c>.
/// Espejo exacto de <c>ListarAgenciaBasesDefectoQuery</c>.
/// </summary>
public sealed record ListarCcsDefectoQuery(
    string Empresa,
    string AlmacenCodigo,
    string AgenciaCodigo)
    : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class ListarCcsDefectoQueryHandler
    : IRequestHandler<
        ListarCcsDefectoQuery,
        ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarCcsDefectoQueryHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ICcRepository ccRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _ccRepository = ccRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        ListarCcsDefectoQuery request,
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

        var agencia =
            await _agenciaRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.AgenciaCodigo,
                cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible " +
                $"para el almacén '{request.AlmacenCodigo}' " +
                $"(empresa '{empresa}').");
        }

        var disponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (!disponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible " +
                $"para el almacén '{request.AlmacenCodigo}' " +
                $"(empresa '{empresa}').");
        }

        var ccs =
            await _ccRepository.ObtenerCcsDefectoAsync(
                empresa,
                request.AlmacenCodigo,
                request.AgenciaCodigo,
                cancellationToken);

        return ccs
            .Select(CrearCcCommandHandler.ToDto)
            .ToList();
    }
}