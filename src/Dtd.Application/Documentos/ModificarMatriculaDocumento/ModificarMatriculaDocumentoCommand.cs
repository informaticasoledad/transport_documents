using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ModificarMatriculaDocumento;

internal sealed class ModificarMatriculaDocumentoCommandHandler
    : IRequestHandler<
        ModificarMatriculaDocumentoCommand,
        ErrorOr<Success>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarMatriculaDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        ModificarMatriculaDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(
            request.DocumentoId,
            cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        try
        {
            documento.ModificarMatricula(request.Matricula);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict(
                "Documento.YaConfirmado",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success;
    }
}