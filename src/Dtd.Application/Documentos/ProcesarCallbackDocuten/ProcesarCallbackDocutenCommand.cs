using System.Text.Json;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ProcesarCallbackDocuten;

public sealed record ProcesarCallbackDocutenCommand(
    JsonElement Payload,
    string RawPayload,
    string? Headers)
    : IRequest<ErrorOr<ProcesarCallbackDocutenResult>>;

public sealed record ProcesarCallbackDocutenResult(
    Guid? DocumentoId,
    string Tipo,
    bool Procesado);
