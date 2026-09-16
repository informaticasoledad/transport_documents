using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.EliminarBaseAgencia;

internal sealed class EliminarBaseAgenciaCommandHandler
    : IRequestHandler<
        EliminarBaseAgenciaCommand,
        ErrorOr<Deleted>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarBaseAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        EliminarBaseAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var agencia =
            await _agenciaRepository.GetByIdConBasesAsync(
                request.AgenciaId,
                cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var baseAgencia = agencia.Bases
            .FirstOrDefault(b => b.Id == request.BaseId);

        if (baseAgencia is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrada",
                $"No existe la base '{request.BaseId}' en la agencia indicada.");
        }


        agencia.EliminarBase(request.BaseId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}