using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ReprocesarCallbackPlataforma;

public sealed record ReprocesarCallbackPlataformaCommand(
    Guid CallbackId,
    bool DryRun = true)
    : IRequest<ErrorOr<ReprocesarCallbackPlataformaResult>>;

public sealed record ReprocesarCallbackPlataformaResult(
    Guid CallbackId,
    Guid? DocumentoId,
    string Tipo,
    bool Procesado,
    bool DryRun,
    string? Mensaje);