using Dtd.Application.AgenciaBases;
using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.AgenciaBases;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EstablecerAgenciaBase;

public sealed record EstablecerAgenciaBaseCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid AgenciaBaseId)
    : IRequest<ErrorOr<AgenciaBaseCatalogoDto>>;

internal sealed class EstablecerAgenciaBaseCommandHandler
    : IRequestHandler<
        EstablecerAgenciaBaseCommand,
        ErrorOr<AgenciaBaseCatalogoDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EstablecerAgenciaBaseCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AgenciaBaseCatalogoDto>> Handle(
        EstablecerAgenciaBaseCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (request.AgenciaBaseId == Guid.Empty)
        {
            return Error.Validation(
                "AgenciaBase.IdRequerido",
                "El agencia base es obligatorio.");
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

        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null || agencia.Empresa != empresa)
        {
            return Error.NotFound(
                "Agencia.NoConfigurada",
                $"La agencia '{request.AgenciaId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        if (agencia.EnvioDirecto)
        {
            return Error.Validation(
                "AlmacenAgencia.AgenciaBaseNoPermitido",
                $"La agencia '{agencia.Codigo}' agrupa por almacén destino " +
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

        var agenciaBase =
            await _agenciaBaseRepository.GetByIdAsync(
                request.AgenciaBaseId,
                cancellationToken);

        if (agenciaBase is null ||
            agenciaBase.Empresa != empresa)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrado",
                $"No existe el agenciaBase '{request.AgenciaBaseId}' " +
                $"para la empresa '{empresa}'.");
        }

        if (!agenciaBase.Activo)
        {
            return Error.Validation(
                "AgenciaBase.Inactivo",
                $"El agenciaBase '{agenciaBase.Codigo}' no está activo.");
        }

        if (!agenciaBase.TieneDireccionCompleta)
        {
            return Error.Validation(
                "AgenciaBase.SinDireccionBase",
                $"El agenciaBase '{agenciaBase.Codigo}' no tiene dirección completa " +
                "para usarlo como base.");
        }

        relacion.ConfigurarAgenciaBase(
            agenciaBase.Id);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearAgenciaBaseCommandHandler.ToDto(
            agenciaBase);
    }
}