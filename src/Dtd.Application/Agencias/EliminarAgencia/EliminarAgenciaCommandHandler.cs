using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.EliminarAgencia;

internal sealed class EliminarAgenciaCommandHandler
    : IRequestHandler<
        EliminarAgenciaCommand,
        ErrorOr<Deleted>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        EliminarAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var tieneDocumentos =
            await _documentoRepository.ExistePorAgenciaAsync(
                request.AgenciaId,
                cancellationToken);

        if (tieneDocumentos)
        {
            return Error.Conflict(
                "Agencia.EnUso",
                "No se puede eliminar la agencia porque tiene documentos asociados.");
        }

        _agenciaRepository.Remove(agencia);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}