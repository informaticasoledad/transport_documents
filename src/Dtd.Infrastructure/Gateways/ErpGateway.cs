using System.Net.Http.Headers;
using System.Text.Json;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure.Gateways;

/// <summary>
/// Real HTTP ERP gateway. The per-company <c>BaseAddress</c> is resolved at call time from
/// <see cref="IEmpresaResolver"/> (backed by the <c>empresas</c> table, cached); everything else
/// of the connection is shared by every company (single OAuth2 client + shared timeout) and comes
/// from <see cref="ErpOptions"/>. The ERP authenticates with a JWT bearer token obtained via the
/// OAuth2 client-credentials grant (<see cref="IEmpresaTokenProvider"/>); the API key header is
/// no longer used.
/// <para>The contract (path and DTO shape) is the same for every company:
/// <c>GET {base}/api/enterprises/{empresa}/expeditions?warehouseId={almacen}&amp;carrierId={agencia}&amp;dateFrom=&amp;dateTo=</c>
/// with an <c>Authorization: Bearer {token}</c> header. Returns a JSON array of expeditions.
/// <c>almacenCodigo</c> maps to the ERP <c>warehouseId</c> (origin warehouse) and
/// <c>agenciaCodigo</c> maps to <c>carrierId</c> (the carrier).</para>
/// </summary>
internal sealed class ErpGateway : IExpedicionErpGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEmpresaResolver _resolver;
    private readonly IEmpresaTokenProvider _tokenProvider;
    private readonly ErpOptions _options;
    private readonly ILogger<ErpGateway> _logger;

    public ErpGateway(
        IHttpClientFactory httpClientFactory,
        IEmpresaResolver resolver,
        IEmpresaTokenProvider tokenProvider,
        IOptions<ErpOptions> options,
        ILogger<ErpGateway> logger)
    {
        _httpClientFactory = httpClientFactory;
        _resolver = resolver;
        _tokenProvider = tokenProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpedicionErpDto>> GetExpedicionesAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        RangoFechas rangoFechas,
        CancellationToken cancellationToken = default)
    {
        var config = await _resolver.ResolveAsync(empresa, cancellationToken)
            ?? throw new EmpresaNoConfiguradaException(empresa);

        // JWT bearer token via OAuth2 client-credentials, cached per empresa.
        var token = await _tokenProvider.GetTokenAsync(config, cancellationToken);

        var client = CreateErpClient(token);

        // URL absoluta al ERP: el base_address de la empresa es la raíz del host (SIN /api), y el
        // path del contrato real empieza por api/enterprises/{empresa}/expeditions. Se construye
        // de forma absoluta (prependiendo el base) para que la URL base quede explícita y no
        // dependa de la combinación BaseAddress+relativa (que con base sin '/' final pierde el
        // último segmento del path). Filtro por warehouseId (almacén de origen) y carrierId
        // (la agencia = carrier), fechas dateFrom/dateTo.
        var url = $"{config.BaseAddress.TrimEnd('/')}/api/enterprises/{Uri.EscapeDataString(empresa)}/expeditions" +
                  $"?warehouseId={Uri.EscapeDataString(almacenCodigo)}" +
                  $"&carrierId={Uri.EscapeDataString(agenciaCodigo)}" +
                  $"&dateFrom={rangoFechas.FechaDesde:yyyy-MM-dd}" +
                  $"&dateTo={rangoFechas.FechaHasta:yyyy-MM-dd}";

        using var response = await client.GetAsync(url, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var expediciones = await JsonSerializer.DeserializeAsync<List<ExpedicionErpDto>>(stream, JsonOptions, cancellationToken);
            if (expediciones is null)
            {
                return new List<ExpedicionErpDto>();
            }

            // empresa no viaja en el body del ERP (es param de query): se estampa aquí. El almacén/agencia
            // se persisten por Id (FK) desde el documento; el gateway ya no estampa el código de agencia.
            return expediciones
                .Select(e => e with { Empresa = empresa })
                .ToList();
        }

        await ThrowErpErrorAsync(response, empresa, $"warehouse={almacenCodigo} carrier={agenciaCodigo}", cancellationToken);
        return new List<ExpedicionErpDto>(); // unreachable: ThrowErpErrorAsync always throws
    }

    /// <summary>Crea el HttpClient nombrado "Erp" con timeout + bearer + Accept/User-Agent (algunos
    /// frontales del ERP rechazan con 403 peticiones que no los llevan aunque el bearer sea válido).</summary>
    private HttpClient CreateErpClient(string token)
    {
        var client = _httpClientFactory.CreateClient("Erp");
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!client.DefaultRequestHeaders.Accept.Any(a => a.MediaType == "application/json"))
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        if (!client.DefaultRequestHeaders.UserAgent.Any())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("dtd-backend", "1.0"));
        }
        return client;
    }

    /// <summary>En error (4xx/5xx) lee el cuerpo del ERP y lo propaga (ErpGatewayException) para que
    /// el handler lo eleve al ProblemDetails y se pueda diagnosticar el motivo real (p.ej. un 403 por
    /// IP no autorizada, por scope, por falta de cabecera...). EnsureSuccessStatusCode tiraba el
    /// cuerpo y solo dejaba el status → 500 opaco. No se loguea el Authorization.</summary>
    private async Task ThrowErpErrorAsync(HttpResponseMessage response, string empresa, string context, CancellationToken cancellationToken)
    {
        var body = await ReadErrorBodyAsync(response.Content, cancellationToken);
        var headers = ReadResponseHeaders(response);
        _logger.LogWarning(
            "ERP respondió {Status} {Reason} para empresa={Empresa} ({Context}). " +
            "Cuerpo: {Body}. Cabeceras: {Headers}",
            (int)response.StatusCode, response.ReasonPhrase, empresa, context, body, headers);
        throw new ErpGatewayException((int)response.StatusCode, response.ReasonPhrase, body, headers);
    }

    /// <summary>
    /// Lee el cuerpo de una respuesta de error del ERP truncándolo a un tamaño seguro (4 KB) para
    /// propagarlo/loguearlo sin riesgo de volcar megabytes. El cuerpo del error del ERP no es un
    /// secret nuestro (es su mensaje de error); el <c>Authorization</c> va en cabecera, no aquí.
    /// </summary>
    private static async Task<string> ReadErrorBodyAsync(HttpContent content, CancellationToken cancellationToken)
    {
        const int maxBytes = 4 * 1024;
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var buffer = new char[maxBytes];
        var read = await reader.ReadBlockAsync(buffer.AsMemory(0, maxBytes), cancellationToken);
        return new string(buffer, 0, read).Trim();
    }

    /// <summary>
    /// Compacta las cabeceras de la respuesta del ERP en "k: v; k: v" para diagnosticar quién deniega
    /// (Server, WWW-Authenticate, Via, X-Powered-By…). Las cabeceras de respuesta del ERP no son
    /// secrets nuestros. Trunca el conjunto a ~1 KB para no inflar el ProblemDetails.
    /// </summary>
    private static string ReadResponseHeaders(HttpResponseMessage response)
    {
        const int maxLen = 1024;
        var sb = new System.Text.StringBuilder();
        foreach (var h in response.Headers.Concat(response.Content.Headers))
        {
            var line = $"{h.Key}: {string.Join(", ", h.Value)}";
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            if (sb.Length + line.Length > maxLen)
            {
                sb.Append("…");
                break;
            }
            sb.Append(line);
        }
        return sb.ToString();
    }
}