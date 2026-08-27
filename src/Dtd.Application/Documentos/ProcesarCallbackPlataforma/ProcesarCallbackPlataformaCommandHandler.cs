using System.Text.Json;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ProcesarCallbackPlataforma;

internal sealed class ProcesarCallbackPlataformaCommandHandler
    : IRequestHandler<ProcesarCallbackPlataformaCommand, ErrorOr<ProcesarCallbackPlataformaResult>>
{
    private const string EventoFirma = "SIGNATURE";
    private const int CallbackLogRetentionDays = 30;

    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocutenCallbackLogRepository _callbackLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcesarCallbackPlataformaCommandHandler(
        IDocumentoRepository documentoRepository,
        IDocutenCallbackLogRepository callbackLogRepository,
        IUnitOfWork unitOfWork)
    {
        _documentoRepository = documentoRepository;
        _callbackLogRepository = callbackLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ProcesarCallbackPlataformaResult>> Handle(
        ProcesarCallbackPlataformaCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Payload.ValueKind != JsonValueKind.Object)
        {
            return await FinalizarAsync(
                request,
                new ProcesarCallbackPlataformaResult(null, "unknown", Procesado: false),
                estado: null,
                mensaje: "Payload no objeto.",
                cancellationToken);
        }

        if (TryGetString(request.Payload, "event") is { } evento &&
            string.Equals(evento, EventoFirma, StringComparison.OrdinalIgnoreCase))
        {
            var resultadoFirma = await ProcesarFirmaAsync(request.Payload, cancellationToken);
            if (resultadoFirma.IsError)
            {
                return resultadoFirma.Errors;
            }

            return await FinalizarAsync(
                request,
                resultadoFirma.Value,
                TryGetString(request.Payload, "shipment_status"),
                mensaje: null,
                cancellationToken);
        }

        var resultadoDocumento = await ProcesarDocumentoAsync(request.Payload, cancellationToken);
        if (resultadoDocumento.IsError)
        {
            return resultadoDocumento.Errors;
        }

        return await FinalizarAsync(
            request,
            resultadoDocumento.Value,
            TryGetString(request.Payload, "lot_status"),
            mensaje: null,
            cancellationToken);
    }

    private async Task<ErrorOr<ProcesarCallbackPlataformaResult>> ProcesarDocumentoAsync(
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var lotReference = TryGetString(payload, "lot_reference");
        var documentoId = TryParseDocumentoId(lotReference);
        if (documentoId is null)
        {
            return new ProcesarCallbackPlataformaResult(null, "document", Procesado: false);
        }

        var documento = await _documentoRepository.GetByIdAsync(documentoId.Value, cancellationToken);
        if (documento is null)
        {
            return new ProcesarCallbackPlataformaResult(null, "document", Procesado: false);
        }

        var callbackAceptado = documento.RegistrarCallbackDocumentoPlataforma(
            TryGetString(payload, "lot_id"),
            TryGetString(payload, "lot_status"));

        if (!callbackAceptado)
        {
            return new ProcesarCallbackPlataformaResult(documento.Id, "document", Procesado: false);
        }

        if (payload.TryGetProperty("shipments", out var shipments) &&
            shipments.ValueKind == JsonValueKind.Array)
        {
            foreach (var shipment in shipments.EnumerateArray())
            {
                documento.RegistrarCallbackEnvioDocuten(
                    TryGetString(shipment, "shipment_reference") ?? string.Empty,
                    TryGetString(shipment, "shipment_id"),
                    TryGetString(shipment, "shipment_status"));
            }
        }

        return new ProcesarCallbackPlataformaResult(documento.Id, "document", Procesado: true);
    }

    private async Task<ErrorOr<ProcesarCallbackPlataformaResult>> ProcesarFirmaAsync(
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var shipmentReference = TryGetString(payload, "shipment_reference");
        var documentoId = TryParseDocumentoId(shipmentReference);
        if (documentoId is null)
        {
            return new ProcesarCallbackPlataformaResult(null, "signature", Procesado: false);
        }

        var documento = await _documentoRepository.GetByIdAsync(documentoId.Value, cancellationToken);
        if (documento is null)
        {
            return new ProcesarCallbackPlataformaResult(documentoId, "signature", Procesado: false);
        }

        var procesado = documento.RegistrarCallbackEnvioDocuten(
            shipmentReference ?? string.Empty,
            TryGetString(payload, "shipment_id"),
            TryGetString(payload, "shipment_status"));

        return new ProcesarCallbackPlataformaResult(documento.Id, "signature", procesado);
    }

    private async Task<ErrorOr<ProcesarCallbackPlataformaResult>> FinalizarAsync(
        ProcesarCallbackPlataformaCommand request,
        ProcesarCallbackPlataformaResult result,
        string? estado,
        string? mensaje,
        CancellationToken cancellationToken)
    {
        var entry = new DocutenCallbackLogEntry(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            result.Tipo,
            result.DocumentoId,
            TryGetString(request.Payload, "lot_id"),
            TryGetString(request.Payload, "lot_reference"),
            TryGetString(request.Payload, "shipment_id"),
            TryGetString(request.Payload, "shipment_reference"),
            TryGetString(request.Payload, "event"),
            estado,
            result.Procesado,
            request.RawPayload,
            request.Headers,
            mensaje);

        await _callbackLogRepository.AddAsync(entry, cancellationToken);

        var threshold = DateTimeOffset.UtcNow.AddDays(-CallbackLogRetentionDays);
        await _callbackLogRepository.DeleteOlderThanAsync(threshold, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return property.GetString();
    }

    private static Guid? TryParseDocumentoId(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return null;
        }

        var idText = reference.Split('#', 2)[0];
        return Guid.TryParse(idText, out var documentoId)
            ? documentoId
            : null;
    }
}
