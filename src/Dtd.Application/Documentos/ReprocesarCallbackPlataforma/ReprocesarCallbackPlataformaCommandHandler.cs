using System.Text.Json;
using Dtd.Application.Documentos.ProcesarCallbackPlataforma;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ReprocesarCallbackPlataforma;

internal sealed class ReprocesarCallbackPlataformaCommandHandler
    : IRequestHandler<
        ReprocesarCallbackPlataformaCommand,
        ErrorOr<ReprocesarCallbackPlataformaResult>>
{
    private readonly IDocutenCallbackLogRepository _callbackRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IMediator _mediator;

    public ReprocesarCallbackPlataformaCommandHandler(
        IDocutenCallbackLogRepository callbackRepository,
        IDocumentoRepository documentoRepository,
        IMediator mediator)
    {
        _callbackRepository = callbackRepository;
        _documentoRepository = documentoRepository;
        _mediator = mediator;
    }

    public async Task<ErrorOr<ReprocesarCallbackPlataformaResult>> Handle(
        ReprocesarCallbackPlataformaCommand request,
        CancellationToken cancellationToken)
    {
        var callback = await _callbackRepository.GetByIdAsync(
            request.CallbackId,
            cancellationToken);

        if (callback is null)
        {
            return Error.NotFound(
                "Docuten.CallbackNoEncontrado",
                "No se encontró el callback solicitado.");
        }

        if (callback.Procesado)
        {
            return Error.Conflict(
                "Docuten.CallbackYaProcesado",
                "El callback ya está procesado.");
        }

        // En esta primera versión recuperamos exclusivamente firmas.
        if (!string.Equals(
                callback.Tipo,
                "signature",
                StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(
                "Docuten.TipoNoSoportado",
                "Solo se permite reprocesar callbacks de firma.");
        }

        JsonDocument json;

        try
        {
            json = JsonDocument.Parse(callback.Payload);
        }
        catch (JsonException)
        {
            return Error.Validation(
                "Docuten.PayloadInvalido",
                "El callback contiene un JSON inválido.");
        }

        using (json)
        {
            var payload = json.RootElement.Clone();

            var referencia = callback.ShipmentReference;

            if (string.IsNullOrWhiteSpace(referencia))
            {
                return Error.Validation(
                    "Docuten.ReferenciaNoInformada",
                    "El callback no contiene la referencia del envío.");
            }

            // Compatibilidad con las referencias antiguas.
            var referenciaDocumento = referencia.Split('#', 2)[0];

            DocumentoDigitalTransporte? documento;

            if (Guid.TryParse(referenciaDocumento, out var documentoId))
            {
                documento = await _documentoRepository.GetByIdAsync(
                    documentoId,
                    cancellationToken);
            }
            else
            {
                documento = await _documentoRepository.GetByReferenciaEnvioAsync(
                    referencia,
                    cancellationToken);
            }

            if (documento is null)
            {
                return Error.NotFound(
                    "Docuten.DocumentoNoEncontrado",
                    $"No se encontró el documento del envío '{referencia}'.");
            }

            if (request.DryRun)
            {
                return new ReprocesarCallbackPlataformaResult(
                    request.CallbackId,
                    documento.Id,
                    callback.Tipo,
                    Procesado: false,
                    DryRun: true,
                    Mensaje: "Simulación correcta: documento localizado. No se han aplicado cambios.");
            }

            var resultado = await _mediator.Send(
                new ProcesarCallbackPlataformaCommand(
                    payload,
                    callback.Payload,
                    callback.Headers,
                    CallbackLogId: callback.Id),
                cancellationToken);

            if (resultado.IsError)
            {
                return resultado.Errors;
            }

            return new ReprocesarCallbackPlataformaResult(
                request.CallbackId,
                resultado.Value.DocumentoId,
                resultado.Value.Tipo,
                resultado.Value.Procesado,
                DryRun: false,
                Mensaje: resultado.Value.Procesado
                    ? "Callback reprocesado correctamente."
                    : "El callback no ha podido aplicarse al documento.");
        }
    }
}