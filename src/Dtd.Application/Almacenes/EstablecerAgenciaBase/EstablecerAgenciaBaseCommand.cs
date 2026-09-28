using Dtd.Application.Agencias;
using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EstablecerAgenciaBase;

public sealed record EstablecerAgenciaBaseCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid AgenciaBaseId)
    : IRequest<ErrorOr<AgenciaBaseDto>>;

internal sealed class EstablecerAgenciaBaseCommandHandler
    : IRequestHandler<
        EstablecerAgenciaBaseCommand,
        ErrorOr<AgenciaBaseDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EstablecerAgenciaBaseCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AgenciaBaseDto>> Handle(
        EstablecerAgenciaBaseCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (request.AgenciaBaseId == Guid.Empty)
        {
            return Error.Validation(
                "AgenciaBase.IdRequerido",
                "La agencia base es obligatoria.");
        }

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

        if (!agencia.Activa)
        {
            return Error.Validation(
                "Agencia.Inactiva",
                $"La agencia '{agencia.Codigo}' no está activa.");
        }

        if (agencia.EntregaEnDestino)
        {
            return Error.Validation(
                "AlmacenAgencia.AgenciaBaseNoPermitido",
                $"La agencia '{agencia.Codigo}' entrega en destino " +
                "y no admite agencia base.");
        }

        var relacion =
            await _almacenRepository.GetRelacionAgenciaParaActualizarAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (relacion is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaId}' no está disponible " +
                $"para el almacén '{request.AlmacenId}' " +
                $"(empresa '{empresa}').");
        }

        var agenciaBase = agencia.Bases
            .FirstOrDefault(x => x.Id == request.AgenciaBaseId);

        if (agenciaBase is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrada",
                $"No existe la base '{request.AgenciaBaseId}' " +
                $"en la agencia '{agencia.Codigo}'.");
        }

        if (!agenciaBase.Activo)
        {
            return Error.Validation(
                "AgenciaBase.Inactiva",
                $"La base '{agenciaBase.Codigo}' no está activa.");
        }

        if (!agenciaBase.TieneDireccionCompleta)
        {
            return Error.Validation(
                "AgenciaBase.SinDireccionBase",
                $"La base '{agenciaBase.Codigo}' no tiene dirección completa " +
                "para usarla como base.");
        }

        relacion.ConfigurarAgenciaBase(agenciaBase.Id);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

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
            //borrar agenciaBase.Movil?.Valor,
            agenciaBase.Email?.Valor,
            //borrar agenciaBase.Canal.Valor,
            agenciaBase.Language,
            agenciaBase.Activo);
    }
}