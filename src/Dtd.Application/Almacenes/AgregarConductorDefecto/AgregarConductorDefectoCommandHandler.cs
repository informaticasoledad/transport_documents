using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.AgregarConductorDefecto;

internal sealed class AgregarConductorDefectoCommandHandler
    : IRequestHandler<
        AgregarConductorDefectoCommand,
        ErrorOr<Success>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AgregarConductorDefectoCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IUnitOfWork unitOfWork)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        AgregarConductorDefectoCommand request,
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

        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                code: "Agencia.NoEncontrada",
                description: "La agencia no existe.");
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

        var conductor = await _conductorRepository.GetByIdAsync(
            request.ConductorId,
            cancellationToken);

        if (conductor is null)
        {
            return Error.NotFound(
                code: "Conductor.NoEncontrado",
                description: "El conductor no existe.");
        }

        // Operación idempotente:
        // si ya está marcado como conductor por defecto,
        // no hacemos nada y devolvemos éxito.
        var yaExiste = almacenAgencia.ConductoresDefecto
            .Any(x => x.ConductorId == request.ConductorId);

        if (yaExiste)
        {
            return Result.Success;
        }

        almacenAgencia.ConductoresDefecto.Add(
            new AlmacenAgenciaConductorDefecto
            {
                AlmacenId = almacen.Id,
                AgenciaId = request.AgenciaId,
                ConductorId = request.ConductorId
            });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}