using Dtd.Application.Agencias;
using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ObtenerAgenciaBaseDefecto;

public sealed record ObtenerAgenciaBaseDefectoQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId)
    : IRequest<ErrorOr<AgenciaBaseDto?>>;

internal sealed class ObtenerAgenciaBaseDefectoQueryHandler
    : IRequestHandler<
        ObtenerAgenciaBaseDefectoQuery,
        ErrorOr<AgenciaBaseDto?>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ObtenerAgenciaBaseDefectoQueryHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AgenciaBaseDto?>> Handle(
        ObtenerAgenciaBaseDefectoQuery request,
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

        var agencia = await _agenciaRepository.GetByIdConBasesAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var relacion =
            await _almacenRepository.GetRelacionAgenciaAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (relacion is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{agencia.Codigo}' no está disponible " +
                $"para el almacén '{almacen.Codigo}'.");
        }

        // Las agencias de envío directo no utilizan agencia base.
        if (agencia.EnvioDirecto)
        {
            return (AgenciaBaseDto?)null;
        }

        // La relación existe, pero todavía no tiene base configurada.
        if (relacion.AgenciaBaseId is not { } agenciaBaseId)
        {
            return (AgenciaBaseDto?)null;
        }

        var agenciaBase = agencia.Bases
            .FirstOrDefault(x => x.Id == agenciaBaseId);

        if (agenciaBase is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrada",
                $"La base configurada para el almacén '{almacen.Codigo}' " +
                $"y la agencia '{agencia.Codigo}' no existe " +
                $"o no pertenece a esa agencia.");
        }

        return ToDto(agenciaBase);
    }

    private static AgenciaBaseDto ToDto(
        AgenciaBase agenciaBase)
    {
        return new AgenciaBaseDto(
            agenciaBase.Id,
            agenciaBase.Codigo,
            agenciaBase.Nombre,
            agenciaBase.TaxId,
            agenciaBase.Direccion,
            agenciaBase.CodigoPostal,
            agenciaBase.Municipio,
            agenciaBase.CodigoPaisIso,
            agenciaBase.Movil?.Valor,
            agenciaBase.Email?.Valor,
            agenciaBase.Canal.Valor,
            agenciaBase.Language,
            agenciaBase.Activo);
    }
}