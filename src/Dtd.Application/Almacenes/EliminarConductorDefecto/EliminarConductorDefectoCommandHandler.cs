using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EliminarConductorDefecto;

internal sealed class EliminarConductorDefectoCommandHandler
    : IRequestHandler<
        EliminarConductorDefectoCommand,
        ErrorOr<Success>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarConductorDefectoCommandHandler(
        IAlmacenRepository almacenRepository,
        IUnitOfWork unitOfWork)
    {
        _almacenRepository = almacenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        EliminarConductorDefectoCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null ||
            !string.Equals(
                almacen.Empresa,
                empresa,
                StringComparison.OrdinalIgnoreCase))
        {
            return Error.NotFound(
                code: "Almacen.NoEncontrado",
                description: "El almacén no existe.");
        }

        var almacenAgencia = await _almacenRepository.GetRelacionAgenciaAsync(
            request.AlmacenId,
            request.AgenciaId,
            cancellationToken);

        if (almacenAgencia is null)
        {
            return Error.Validation(
                code: "AlmacenAgencia.NoVinculada",
                description: "La agencia no está vinculada al almacén.");
        }

        var conductorDefecto = almacenAgencia.ConductoresDefecto
            .FirstOrDefault(x =>
                x.ConductorId == request.ConductorId);

        // Idempotente: si no existe, consideramos que ya está eliminado.
        if (conductorDefecto is null)
        {
            return Result.Success;
        }

        almacenAgencia.ConductoresDefecto.Remove(conductorDefecto);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}