using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.EliminarConductor;

internal sealed class EliminarConductorCommandHandler
    : IRequestHandler<
        EliminarConductorCommand,
        ErrorOr<Deleted>>
{
    private readonly IConductorRepository _conductorRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarConductorCommandHandler(
        IConductorRepository conductorRepository,
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork)
    {
        _conductorRepository = conductorRepository;
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        EliminarConductorCommand request,
        CancellationToken cancellationToken)
    {
        var conductor = await _conductorRepository.GetByIdAsync(
            request.ConductorId,
            cancellationToken);

        if (conductor is null)
        {
            return Error.NotFound(
                "Conductor.NoEncontrado",
                $"No existe el conductor '{request.ConductorId}'.");
        }

        var tieneDocumentos =
            await _documentoRepository.ExistePorConductorAsync(
                request.ConductorId,
                cancellationToken);

        if (tieneDocumentos)
        {
            return Error.Conflict(
                "Conductor.EnUso",
                "No se puede eliminar el conductor porque está asociado a documentos.");
        }

        _conductorRepository.Remove(conductor);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Deleted;
    }
}