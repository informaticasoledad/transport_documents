using Dtd.Application.Almacenes;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Mapping;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using Mapster;
using MediatR;

namespace Dtd.Application.Documentos.ListarExpedicionesDisponibles;

/// <summary>
/// Returns the expeditions available to be included in a new document:
/// those coming from the ERP for the company/agency and date range
/// that are not yet included in any existing document.
/// </summary>
public sealed record ListarExpedicionesDisponiblesQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    DateOnly FechaDesde,
    DateOnly FechaHasta)
    : IRequest<ErrorOr<IReadOnlyList<ExpedicionDto>>>;

internal sealed class ListarExpedicionesDisponiblesQueryHandler
    : IRequestHandler<
        ListarExpedicionesDisponiblesQuery,
        ErrorOr<IReadOnlyList<ExpedicionDto>>>
{
    private readonly IExpedicionErpGateway _erpGateway;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarExpedicionesDisponiblesQueryHandler(
        IExpedicionErpGateway erpGateway,
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _erpGateway = erpGateway;
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<ExpedicionDto>>> Handle(
        ListarExpedicionesDisponiblesQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                empresa,
                request.AlmacenId,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        var rango = RangoFechas.Create(
            request.FechaDesde,
            request.FechaHasta);

        // Validación contra los maestros locales.
        var almacen =
            await _almacenRepository.GetByIdAsync(
                request.AlmacenId,
                cancellationToken);

        if (almacen is null ||
            almacen.Empresa != empresa)
        {
            return Error.Validation(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var agencia =
            await _agenciaRepository.GetByIdAsync(
                request.AgenciaId,
                cancellationToken);

        if (agencia is null ||
            agencia.Empresa != empresa)
        {
            return Error.Validation(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var agenciaDisponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (!agenciaDisponible)
        {
            return Error.Validation(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{agencia.Codigo}' no está disponible " +
                $"para el almacén '{almacen.Codigo}' " +
                $"(empresa '{empresa}').");
        }

        IReadOnlyList<ExpedicionErpDto> expedicionesErp;

        try
        {
            expedicionesErp =
                await _erpGateway.GetExpedicionesAsync(
                    empresa,
                    almacen.Codigo,
                    agencia.Codigo,
                    rango,
                    cancellationToken);
        }
        catch (EmpresaNoConfiguradaException ex)
        {
            return Error.Failure(
                "Empresa.ErpNoConfigurado",
                ex.Message);
        }
        catch (ErpGatewayException ex)
        {
            return ErpGatewayErrorMapper.ToError(ex);
        }

        if (expedicionesErp.Count == 0)
        {
            return new List<ExpedicionDto>();
        }

        var erpIds = expedicionesErp
            .Select(e => e.Id)
            .ToList();

        var yaIncluidos =
            await _documentoRepository.ObtenerErpIdsIncluidosAsync(
                empresa,
                almacen.Id,
                agencia.Id,
                erpIds,
                cancellationToken);

        var disponibles = expedicionesErp
            .Where(e => !yaIncluidos.Contains(e.Id))
            .Select(e =>
                e.ToDomain(
                        almacen.Id,
                        agencia.Id)
                    .Adapt<ExpedicionDto>())
            .ToList();

        return disponibles;
    }
}