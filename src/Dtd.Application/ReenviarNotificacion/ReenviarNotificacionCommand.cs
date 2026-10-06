using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ReenviarNotificacion;

public sealed record ReenviarNotificacionCommand(
    Guid DocumentoId,
    Guid EnvioId)
    : IRequest<ErrorOr<Success>>;