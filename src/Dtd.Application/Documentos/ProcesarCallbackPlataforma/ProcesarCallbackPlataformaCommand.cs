using System.Text.Json;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ProcesarCallbackPlataforma;

public sealed record ProcesarCallbackPlataformaCommand(
    JsonElement Payload,
    string RawPayload,
    string? Headers)
    : IRequest<ErrorOr<ProcesarCallbackPlataformaResult>>;

public sealed record ProcesarCallbackPlataformaResult(
    Guid? DocumentoId,
    string Tipo,
    bool Procesado);
