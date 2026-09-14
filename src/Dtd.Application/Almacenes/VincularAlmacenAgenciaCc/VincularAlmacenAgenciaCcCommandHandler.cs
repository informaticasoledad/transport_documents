
    using Dtd.Application.Ccs;
    using Dtd.Domain.Agencias;
    using Dtd.Domain.Almacenes;
    using Dtd.Domain.Ccs;
    using Dtd.Domain.Common;
    using ErrorOr;
    using MediatR;

    namespace Dtd.Application.Almacenes.VincularAlmacenAgenciaCc;

internal sealed class VincularAlmacenAgenciaCcCommandHandler
    : IRequestHandler<
        VincularAlmacenAgenciaCcCommand,
        ErrorOr<CcCatalogoDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public VincularAlmacenAgenciaCcCommandHandler(
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
        VincularAlmacenAgenciaCcCommand request,
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

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var agenciaDisponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (!agenciaDisponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{agencia.Id}' no está disponible " +
                $"para el almacén '{almacen.Id}'.");
        }

        var cc = await _ccRepository.GetByIdAsync(
            request.CcId,
            cancellationToken);

        if (cc is null || cc.Empresa != empresa)
        {
            return Error.NotFound(
                "Cc.NoEncontrado",
                $"El CC '{request.CcId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var ccVinculado =
            await _ccRepository.GetByAlmacenYAgenciaEIdAsync(
                almacen.Id,
                agencia.Id,
                cc.Id,
                cancellationToken);

        if (ccVinculado is not null)
        {
            return Error.Conflict(
                "Cc.YaVinculado",
                $"El CC '{cc.Id}' ya está vinculado al almacén " +
                $"'{almacen.Id}' y la agencia '{agencia.Id}'.");
        }

        await _ccRepository.AgregarVinculoAsync(
            cc.Id,
            almacen.Id,
            agencia.Id,
            request.PorDefecto,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearCcCommandHandler.ToDto(cc);
    }
}
