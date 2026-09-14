using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs.AgregarCcDefecto;

/// <summary>
/// Añade un CC como CC por defecto para una combinación
/// (empresa, almacén, agencia).
/// </summary>

internal sealed class AgregarCcDefectoCommandHandler
    : IRequestHandler<
        AgregarCcDefectoCommand,
        ErrorOr<CcCatalogoDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public AgregarCcDefectoCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<CcCatalogoDto>> Handle(
        AgregarCcDefectoCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (request.CcId == Guid.Empty)
        {
            return Error.Validation(
                "Cc.IdInvalido",
                "El identificador del CC no es válido.");
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

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
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

        var cc =
            await _ccRepository.GetByAlmacenYAgenciaEIdAsync(
                almacen.Id,
                agencia.Id,
                request.CcId,
                cancellationToken);

        if (cc is null)
        {
            return Error.NotFound(
                "Cc.NoVinculado",
                $"El CC '{request.CcId}' no está vinculado al almacén " +
                $"'{request.AlmacenId}' y la agencia " +
                $"'{request.AgenciaId}'.");
        }

        await _ccRepository.AgregarDefectoAsync(
            almacen.Id,
            agencia.Id,
            cc.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearCcCommandHandler.ToDto(cc);
    }
}