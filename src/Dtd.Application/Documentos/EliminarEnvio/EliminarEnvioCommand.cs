using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EliminarEnvio;

public sealed record EliminarEnvioCommand(
    Guid DocumentoId,
    Guid EnvioId)
    : IRequest<ErrorOr<Deleted>>;