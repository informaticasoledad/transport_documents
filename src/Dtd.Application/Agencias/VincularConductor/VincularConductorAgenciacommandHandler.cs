using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.VincularConductor;

internal sealed class VincularConductorAgenciaCommandHandler
    : IRequestHandler<
        VincularConductorAgenciaCommand,
        ErrorOr<Success>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VincularConductorAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        VincularConductorAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                code: "Agencia.NoEncontrada",
                description: "La agencia no existe.");
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

        var yaExiste =
            await _conductorRepository.ExisteRelacionAgenciaAsync(
                request.ConductorId,
                request.AgenciaId,
                cancellationToken);

        if (yaExiste)
        {
            return Result.Success;
        }

        var relacion = new ConductorAgencia
        {
            ConductorId = request.ConductorId,
            AgenciaId = request.AgenciaId
        };

        await _conductorRepository.AgregarRelacionAgenciaAsync(
            relacion,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success;
    }
}