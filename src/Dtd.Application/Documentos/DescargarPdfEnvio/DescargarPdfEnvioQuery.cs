using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.DescargarPdfEnvio;

public sealed record DescargarPdfEnvioQuery(
    Guid DocumentoId,
    Guid EnvioId)
    : IRequest<ErrorOr<DocumentoPdfResult>>;