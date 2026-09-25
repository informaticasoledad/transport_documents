using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EliminarDocumento;

public sealed record EliminarDocumentoCommand(
    Guid DocumentoId)
    : IRequest<ErrorOr<Deleted>>;