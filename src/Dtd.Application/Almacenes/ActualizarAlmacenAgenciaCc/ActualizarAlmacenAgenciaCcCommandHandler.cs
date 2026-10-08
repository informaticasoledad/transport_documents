using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ActualizarAlmacenAgenciaCc;

internal sealed class ActualizarAlmacenAgenciaCcCommandHandler
    : IRequestHandler<
        ActualizarAlmacenAgenciaCcCommand,
        ErrorOr<Success>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ActualizarAlmacenAgenciaCcCommandHandler(
        IAlmacenRepository almacenRepository,
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<Success>> Handle(
        ActualizarAlmacenAgenciaCcCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        // 1. Validar almacén y empresa
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

        // 2. Validar acceso al almacén
        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                empresa,
                almacen.Id,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        // 3. Comprobar que la agencia está disponible
        var agenciaDisponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                request.AgenciaId,
                cancellationToken);

        if (!agenciaDisponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                "La agencia no está disponible para este almacén.");
        }

        // 4. Validar que el CC pertenece a la empresa
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

        // 5. Actualizar PorDefecto
        var actualizado = await _ccRepository.ActualizarVinculoAsync(
            cc.Id,
            almacen.Id,
            request.AgenciaId,
            request.PorDefecto,
            cancellationToken);

        if (!actualizado)
        {
            return Error.NotFound(
                "Cc.VinculoNoEncontrado",
                "El CC no está vinculado a esta agencia y almacén.");
        }

        // 6. Persistir cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}