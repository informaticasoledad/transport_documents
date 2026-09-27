using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EliminarExpedicion;

public sealed record EliminarExpedicionCommand(
    Guid DocumentoId,
    Guid EnvioId,
    string ExpedicionErpId)
    : IRequest<ErrorOr<Deleted>>;