using Dtd.Application.Conductores;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarConductoresDefecto;

/// <summary>
/// Lista los conductores por defecto de una tupla
/// (empresa, almacén, agencia) para que el front los
/// auto-adjunte al generar un documento para esa tupla.
/// Pueden ser varios; sólo los activos.
/// El back no los auto-adjunta: los añade el front vía
/// POST /documentos/{id}/conductores.
/// </summary>
public sealed record ListarConductoresDefectoQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId)
    : IRequest<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>;

internal sealed class ListarConductoresDefectoQueryHandler
    : IRequestHandler<
        ListarConductoresDefectoQuery,
        ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarConductoresDefectoQueryHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>> Handle(
        ListarConductoresDefectoQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        // El almacén debe existir.
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

        // El usuario debe tener acceso al almacén concreto.
        var accesoAlmacen = await _accesoAlmacenService.ValidarAccesoAsync(
            empresa,
            almacen.Id,
            cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        // La agencia debe existir y pertenecer a la empresa.
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

        // Además debe estar vinculada al almacén.
        var disponible = await _almacenRepository.EsAgenciaDisponibleAsync(
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

        var conductores =
            await _conductorRepository.ObtenerConductoresDefectoAsync(
                empresa,
                almacen.Id,
                agencia.Id,
                cancellationToken);

        return conductores
            .Select(c => new ConductorCatalogoDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Movil = c.Movil?.Valor,
                Language = c.Language
            })
            .ToList();
    }
}