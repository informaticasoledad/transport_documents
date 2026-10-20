using ErrorOr;
using MediatR;

namespace Dtd.Application.Docuten.ReprocesarCallbacksPendientes;

public sealed record ReprocesarCallbacksPendientesCommand(
    bool DryRun = true,
    int Limit = 10)
    : IRequest<ErrorOr<ReprocesarCallbacksPendientesResult>>;

public sealed record ReprocesarCallbacksPendientesResult(
    bool DryRun,
    int Seleccionados,
    int Procesados,
    int NoProcesados,
    IReadOnlyList<ResultadoReprocesamientoCallback> Resultados);

public sealed record ResultadoReprocesamientoCallback(
    Guid CallbackId,
    Guid? DocumentoId,
    bool Procesado,
    string? Mensaje);