using Dtd.Application.Documentos.ReprocesarCallbackPlataforma;
using Dtd.Application.GatewayContracts;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Docuten.ReprocesarCallbacksPendientes;

internal sealed class ReprocesarCallbacksPendientesCommandHandler
    : IRequestHandler<
        ReprocesarCallbacksPendientesCommand,
        ErrorOr<ReprocesarCallbacksPendientesResult>>
{
    private readonly IDocutenCallbackLogRepository _callbackLogRepository;
    private readonly IMediator _mediator;

    public ReprocesarCallbacksPendientesCommandHandler(
        IDocutenCallbackLogRepository callbackLogRepository,
        IMediator mediator)
    {
        _callbackLogRepository = callbackLogRepository;
        _mediator = mediator;
    }

    public async Task<ErrorOr<ReprocesarCallbacksPendientesResult>> Handle(
        ReprocesarCallbacksPendientesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Limit is < 1 or > 100)
        {
            return Error.Validation(
                "Callbacks.LimitInvalido",
                "El límite debe estar entre 1 y 100.");
        }

        var pendientes = await _callbackLogRepository.ListarPendientesAsync(
            request.Limit,
            cancellationToken);

        var resultados = new List<ResultadoReprocesamientoCallback>();

        foreach (var callback in pendientes)
        {
            var resultado = await _mediator.Send(
                new ReprocesarCallbackPlataformaCommand(
                    callback.Id,
                    request.DryRun),
                cancellationToken);

            if (resultado.IsError)
            {
                // Detenemos el lote ante un error.
                // Los callbacks anteriores ya pueden estar guardados.
                return resultado.Errors;
            }

            var value = resultado.Value;

            resultados.Add(new ResultadoReprocesamientoCallback(
                callback.Id,
                value.DocumentoId,
                value.Procesado,
                value.Mensaje));
        }

        return new ReprocesarCallbacksPendientesResult(
            request.DryRun,
            resultados.Count,
            resultados.Count(x => x.Procesado),
            resultados.Count(x => !x.Procesado),
            resultados);
    }
}