using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;
using Dtd.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure.Gateways;

/// <summary>
/// Real HTTP Docuten gateway. Contrato real:
/// - <c>POST {base}/api/v1/lots</c> (async): crea un lote con N shipments, devuelve <c>lot_id</c> en estado
///   <c>pending</c> y procesa en background notificando a <c>callback_url</c>.
/// - <c>GET {base}/api/v1/lots/{lotId}</c>: sondea el estado del lote y sus shipments.
/// Auth: cabecera <c>X-API-KEY</c> (secret env-only <c>Docuten:TokenId</c>). 1 DDT = 1 lote.
/// </summary>
internal sealed class DocutenGateway : IDocutenGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        // Los [JsonPropertyName] de los DTOs dictan los nombres en wire (snake_case); case-insensitive al leer.
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Omite los campos null al serializar la petición (alinea el wire con el envío exitoso de
        // docs/request.json, que no envía campos vacíos). No afecta a la deserialización de respuestas
        // ni a callback_url (que se envía como "" — string vacío, no null— y por tanto sí se emite).
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly DocutenOptions _options;
    private readonly ILogger<DocutenGateway> _logger;

    public DocutenGateway(HttpClient httpClient, IOptions<DocutenOptions> options, ILogger<DocutenGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.BaseAddress);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // Docuten eCMR autentica con X-API-KEY (no bearer). El valor es un secret env-only
        // (Docuten:TokenId → user-secrets en dev / Docuten__TokenId en k8s). Lo valida ValidateOnStart
        // (UseMock=false ⇒ obligatorio); este throw es defense-in-depth por si se resolviera sin startup.
        if (string.IsNullOrWhiteSpace(_options.TokenId))
        {
            throw new InvalidOperationException(
                "Falta el API key de Docuten: inyéctalo por 'Docuten:TokenId' (user-secrets en dev / " +
                "env var 'Docuten__TokenId' desde un Secret de k8s en prod). Es obligatorio cuando " +
                "Docuten:UseMock=false.");
        }
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _options.TokenId);
    }

    public async Task<DocutenLoteEnvioResult> EnviarAsync(DocutenLoteDto lote, CancellationToken cancellationToken = default)
    {
        var content = JsonContent.Create(lote, options: JsonOptions);

        // Diagnóstico (Docuten:LogRawPayload): loguea el body EXACTO que enviamos. Útil para ver qué
        // campo rechaza Docuten en silencio (2xx sin lote en sandbox) o para cotejar contra el contrato.
        if (_options.LogRawPayload)
        {
            var rawRequest = JsonSerializer.Serialize(lote, JsonOptions);
            _logger.LogInformation("Docuten POST /api/v1/lots request: {Request}", rawRequest);
        }

        using var response = await _httpClient.PostAsync("api/v1/lots", content, cancellationToken);
        var rawResponse = await ReadResponseAsync(response, "POST /api/v1/lots", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new DocutenGatewayException((int)response.StatusCode, rawResponse);
        }

        var json = JsonSerializer.Deserialize<DocutenLotCreateResponseJson>(rawResponse, JsonOptions)
            ?? throw new InvalidOperationException("Docuten devolvió una respuesta vacía al crear el lote.");

        // Docuten puede devolver 2xx (incluso con lot_id) Y a la vez un sobre de error
        // (error_code/error_message/details) cuando el lote tiene errores de validación: el lote no se
        // crea realmente y no aparece en el sandbox. Sin este chequeo, recogeríamos el lot_id y el
        // handler marcaría el documento Enviando sobre un envío que en realidad falló. Si hay
        // error_code, tratamos la respuesta como fallo (lanza → el handler registra intento fallido y
        // mantiene el documento en Nuevo para reintentar), aunque el HTTP status sea 2xx.
        if (!string.IsNullOrWhiteSpace(json.ErrorCode))
        {
            throw new DocutenGatewayException((int)response.StatusCode, rawResponse);
        }

        // Si Docuten responde 2xx pero SIN lot_id, el lote no se creó realmente: volcamos el body
        // completo en el mensaje (no sólo "no devolvió lot_id") para poder diagnosticar por qué.
        var lotId = json.LotId ?? throw new InvalidOperationException(
            $"Docuten no devolvió lot_id al crear el lote. Respuesta completa: {rawResponse}");
        var estado = json.Status ?? json.LotStatus ?? EstadoDocuten.Pending;
        var shipments = (json.Shipments ?? [])
            .Select(s => new DocutenShipmentEnvioResult(
                s.ShipmentReference,
                s.ShipmentId,
                s.ShipmentStatus ?? EstadoDocuten.Pending))
            .ToList();

        return new DocutenLoteEnvioResult(lotId, estado, shipments);
    }

    public async Task<DocutenLoteEstadoResult> ObtenerEstadoAsync(string lotId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/v1/lots/{Uri.EscapeDataString(lotId)}", cancellationToken);
        var rawResponse = await ReadResponseAsync(response, $"GET /api/v1/lots/{lotId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new DocutenGatewayException((int)response.StatusCode, rawResponse);
        }

        var json = JsonSerializer.Deserialize<DocutenLotStatusResponseJson>(rawResponse, JsonOptions)
            ?? throw new InvalidOperationException("Docuten devolvió una respuesta vacía al sondear el lote.");

        var estado = CoalesceLotStatus(json);
        var shipments = (json.Shipments ?? [])
            .Select(s => new DocutenShipmentEstadoDto
            {
                ShipmentId = s.ShipmentId,
                ShipmentStatus = s.ShipmentStatus ?? EstadoDocuten.Pending,
                SignatureStatus = s.SignatureStatus,
                ProofOfDelivery = s.ProofOfDelivery
            })
            .ToList();

        return new DocutenLoteEstadoResult(estado, shipments);
    }

    public async Task CancelarAsync(string lotId, string reason, CancellationToken cancellationToken = default)
    {
        // Los shipment_id se obtienen sondeando el lote (no los persistimos). Se cancelan sólo los
        // shipments en estado cancelable; los terminales se ignoran (ya no se pueden cancelar).
        var estado = await ObtenerEstadoAsync(lotId, cancellationToken);

        var cancelables = estado.Shipments
            .Where(s => !string.IsNullOrWhiteSpace(s.ShipmentId) && EsCancelable(s.ShipmentStatus))
            .Select(s => s.ShipmentId!)
            .ToList();

        foreach (var shipmentId in cancelables)
        {
            var body = JsonContent.Create(new DocutenCancelShipmentRequest { Reason = reason }, options: JsonOptions);
            using var response = await _httpClient.PostAsync(
                $"api/v1/shipments/{Uri.EscapeDataString(shipmentId)}/cancel", body, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }
    }

    /// <summary>Un shipment es cancelable mientras no haya llegado a un estado terminal
    /// (<c>delivered</c>/<c>completed</c>/<c>cancelled</c>/<c>error</c>).</summary>
    private static bool EsCancelable(string shipmentStatus) => shipmentStatus switch
    {
        EstadoDocuten.Pending => true,
        EstadoDocuten.Created => true,
        EstadoDocuten.ReadyForPickup => true,
        EstadoDocuten.PendingDelivery => true,
        _ => false
    };

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await ReadErrorBodyAsync(response, cancellationToken);
        throw new DocutenGatewayException((int)response.StatusCode, body);
    }

    /// <summary>Lee el body **completo** de la respuesta y, si <see cref="DocutenOptions.LogRawPayload"/>
    /// está activo, lo loguea (Information) junto al status. Devuelve el body crudo para que el llamante
    /// lo deserialice o lo incluya en el error. A diferencia de <see cref="ReadErrorBodyAsync"/> (que
    /// trunca a 4 KB y sólo se usa en <c>CancelarAsync</c>), ésta no trunca: es la vía de diagnóstico
    /// para ver íntegramente qué responde Docuten (incluidos campos que el DTO no mapea).</summary>
    private async Task<string> ReadResponseAsync(HttpResponseMessage response, string label, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (_options.LogRawPayload)
        {
            _logger.LogInformation(
                "Docuten {Label} → {Status} {Reason}. Response: {Response}",
                label, (int)response.StatusCode, response.ReasonPhrase ?? string.Empty, body);
        }
        return body;
    }

    private static async Task<string> ReadErrorBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            var buffer = new char[4096];
            var read = await reader.ReadAsync(buffer, cancellationToken);
            return new string(buffer, 0, read).Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>Coalescea el estado del lote: si trae status/lot_status lo usa; si no, aplanea desde los
    /// shipments (mínimo progreso; error si alguno falla; cancelled si todos cancelados).</summary>
    private static string CoalesceLotStatus(DocutenLotStatusResponseJson json)
    {
        var lotStatus = json.Status ?? json.LotStatus;
        if (!string.IsNullOrWhiteSpace(lotStatus))
        {
            return lotStatus!;
        }

        var shipments = json.Shipments;
        if (shipments is null || shipments.Count == 0)
        {
            return EstadoDocuten.Pending;
        }

        var statuses = shipments
            .Select(s => s.ShipmentStatus)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!)
            .ToList();

        if (statuses.Count == 0)
        {
            return EstadoDocuten.Pending;
        }

        if (statuses.Any(s => s == EstadoDocuten.Error))
        {
            return EstadoDocuten.Error;
        }

        if (statuses.All(s => s == EstadoDocuten.Cancelled))
        {
            return EstadoDocuten.Cancelled;
        }

        var order = new (string Status, int Order)[]
        {
            (EstadoDocuten.Pending, 0),
            (EstadoDocuten.Created, 1),
            (EstadoDocuten.ReadyForPickup, 2),
            (EstadoDocuten.PendingDelivery, 3),
            (EstadoDocuten.Delivered, 4),
            (EstadoDocuten.Completed, 5)
        };

        var minOrder = statuses
            .Where(s => s != EstadoDocuten.Cancelled && s != EstadoDocuten.Error)
            .Select(s => order.FirstOrDefault(o => o.Status == s).Order)
            .DefaultIfEmpty(0)
            .Min();

        return order.First(o => o.Order == minOrder).Status;
    }

    // --- DTOs JSON internos de respuesta (snake_case del contrato real) ---

    private sealed record DocutenLotCreateResponseJson
    {
        [JsonPropertyName("lot_id")] public string? LotId { get; init; }
        [JsonPropertyName("status")] public string? Status { get; init; }
        [JsonPropertyName("lot_status")] public string? LotStatus { get; init; }
        [JsonPropertyName("shipments")] public List<DocutenShipmentCreateJson>? Shipments { get; init; }
        // Docuten usa este sobre (error_code/error_message/details) incluso en respuestas 2xx cuando
        // el lote tiene errores de validación. Su presencia significa que el envío NO fue aceptado.
        [JsonPropertyName("error_code")] public string? ErrorCode { get; init; }
    }

    private sealed record DocutenShipmentCreateJson
    {
        [JsonPropertyName("shipment_id")] public string? ShipmentId { get; init; }
        [JsonPropertyName("shipment_reference")] public string? ShipmentReference { get; init; }
        [JsonPropertyName("shipment_status")] public string? ShipmentStatus { get; init; }
    }

    private sealed record DocutenLotStatusResponseJson
    {
        [JsonPropertyName("lot_id")] public string? LotId { get; init; }
        [JsonPropertyName("status")] public string? Status { get; init; }
        [JsonPropertyName("lot_status")] public string? LotStatus { get; init; }
        [JsonPropertyName("shipments")] public List<DocutenShipmentStatusJson>? Shipments { get; init; }
    }

    private sealed record DocutenShipmentStatusJson
    {
        [JsonPropertyName("shipment_id")] public string? ShipmentId { get; init; }
        [JsonPropertyName("shipment_status")] public string? ShipmentStatus { get; init; }
        [JsonPropertyName("shipment_signature_status")] public string? SignatureStatus { get; init; }
        [JsonPropertyName("proof_of_delivery")] public string? ProofOfDelivery { get; init; }
    }
}
