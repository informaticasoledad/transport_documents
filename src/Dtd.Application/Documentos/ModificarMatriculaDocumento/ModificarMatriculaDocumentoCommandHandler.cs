using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ModificarMatriculaDocumento;

public sealed record ModificarMatriculaDocumentoCommand(
    Guid DocumentoId,
    string? Matricula,
    string? MatriculaRemolque)
    : IRequest<ErrorOr<Success>>;