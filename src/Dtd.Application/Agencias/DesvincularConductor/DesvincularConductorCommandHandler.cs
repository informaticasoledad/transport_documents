using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.DesvincularConductor;

internal sealed class DesvincularConductorAgenciaCommandHandler
    : IRequestHandler<
        DesvincularConductorAgenciaCommand,
        ErrorOr<Success>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DesvincularConductorAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DesvincularConductorAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                "La agencia no existe.");
        }

        var conductor = await _conductorRepository.GetByIdAsync(
            request.ConductorId,
            cancellationToken);

        if (conductor is null)
        {
            return Error.NotFound(
                "Conductor.NoEncontrado",
                "El conductor no existe.");
        }

        var relacion =
            await _conductorRepository.ObtenerRelacionAgenciaAsync(
                request.ConductorId,
                request.AgenciaId,
                cancellationToken);

        // Idempotente: si ya no está relacionado, no hacemos nada.
        if (relacion is null)
        {
            return Result.Success;
        }

        // Primero eliminamos cualquier conductor por defecto
        // para esta agencia en cualquier almacén.
        var asignacionesDefecto =
            await _conductorRepository
                .ObtenerAsignacionesDefectoPorAgenciaConductorAsync(
                    request.AgenciaId,
                    request.ConductorId,
                    cancellationToken);

        if (asignacionesDefecto.Count > 0)
        {
            _conductorRepository.EliminarAsignacionesDefecto(
                asignacionesDefecto);
        }

        // Después eliminamos conductor_agencias.
        _conductorRepository.EliminarRelacionAgencia(
            relacion);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success;
    }
}