using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ModificarMatriculaDocumento;

public sealed record ModificarMatriculaDocumentoCommand(
    Guid DocumentoId,
    string? Matricula)
    : IRequest<ErrorOr<Success>>;