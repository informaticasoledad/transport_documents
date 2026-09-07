using Dtd.Application.Ccs;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarCcsDefecto;

/// <summary>
/// Lista los CCs por defecto de una tupla (empresa, almacén, agencia)
/// para que el front los auto-adjunte al generar un documento.
/// El back no los auto-adjunta; los añade el front vía
/// <c>POST /documentos/{id}/ccs</c>.
/// </summary>
public sealed record ListarCcsDefectoQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId)
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

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null || almacen.Empresa != empresa)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenId}' no existe " +
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

        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null || agencia.Empresa != empresa)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaId}' no está disponible " +
                $"para el almacén '{request.AlmacenId}' " +
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
                $"La agencia '{request.AgenciaId}' no está disponible " +
                $"para el almacén '{request.AlmacenId}' " +
                $"(empresa '{empresa}').");
        }

        var ccs =
            await _ccRepository.ObtenerCcsDefectoAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        return ccs
            .Select(CrearCcCommandHandler.ToDto)
            .ToList();
    }
}