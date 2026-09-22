using System.Net.Http.Headers;
using System.Text.Json;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure.Gateways;

internal sealed class ErpGateway : IExpedicionErpGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IEmpresaTokenProvider _tokenProvider;
    private readonly ErpOptions _options;
    private readonly ILogger<ErpGateway> _logger;

    public ErpGateway(
        IHttpClientFactory httpClientFactory,
        IEmpresaRepository empresaRepository,
        IEmpresaTokenProvider tokenProvider,
        IOptions<ErpOptions> options,
        ILogger<ErpGateway> logger)
    {
        _httpClientFactory = httpClientFactory;
        _empresaRepository = empresaRepository;
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
        var config =
            await _empresaRepository.GetByEmpresaAsync(
                empresa,
                cancellationToken)
            ?? throw new EmpresaNoConfiguradaException(empresa);

        var token = await _tokenProvider.GetTokenAsync(
            config,
            cancellationToken);

        var client = CreateErpClient(token);

        var url =
            $"{config.BaseAddress.TrimEnd('/')}" +
            $"/api/enterprises/{Uri.EscapeDataString(empresa)}/expeditions" +
            $"?warehouseId={Uri.EscapeDataString(almacenCodigo)}" +
            $"&carrierId={Uri.EscapeDataString(agenciaCodigo)}" +
            $"&dateFrom={rangoFechas.FechaDesde:yyyy-MM-dd}" +
            $"&dateTo={rangoFechas.FechaHasta:yyyy-MM-dd}";

        using var response =
            await client.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            await using var stream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            var expediciones =
                await JsonSerializer.DeserializeAsync<
                    List<ExpedicionErpDto>>(
                        stream,
                        JsonOptions,
                        cancellationToken);

            if (expediciones is null)
            {
                return [];
            }

            return expediciones
                .Select(e => e with
                {
                    Empresa = empresa
                })
                .ToList();
        }

        await ThrowErpErrorAsync(
            response,
            empresa,
            $"warehouse={almacenCodigo} carrier={agenciaCodigo}",
            cancellationToken);

        return [];
    }

    private HttpClient CreateErpClient(string token)
    {
        var client =
            _httpClientFactory.CreateClient("Erp");

        client.Timeout =
            TimeSpan.FromSeconds(_options.TimeoutSeconds);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        if (!client.DefaultRequestHeaders.Accept.Any(
                a => a.MediaType == "application/json"))
        {
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));
        }

        if (!client.DefaultRequestHeaders.UserAgent.Any())
        {
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "dtd-backend",
                    "1.0"));
        }

        return client;
    }

    private async Task ThrowErpErrorAsync(
        HttpResponseMessage response,
        string empresa,
        string context,
        CancellationToken cancellationToken)
    {
        var body =
            await ReadErrorBodyAsync(
                response.Content,
                cancellationToken);

        var headers =
            ReadResponseHeaders(response);

        _logger.LogWarning(
            "ERP respondió {Status} {Reason} para empresa={Empresa} ({Context}). " +
            "Cuerpo: {Body}. Cabeceras: {Headers}",
            (int)response.StatusCode,
            response.ReasonPhrase,
            empresa,
            context,
            body,
            headers);

        throw new ErpGatewayException(
            (int)response.StatusCode,
            response.ReasonPhrase,
            body,
            headers);
    }

    private static async Task<string> ReadErrorBodyAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        const int maxBytes = 4 * 1024;

        await using var stream =
            await content.ReadAsStreamAsync(
                cancellationToken);

        using var reader =
            new StreamReader(stream);

        var buffer =
            new char[maxBytes];

        var read =
            await reader.ReadBlockAsync(
                buffer.AsMemory(0, maxBytes),
                cancellationToken);

        return new string(
            buffer,
            0,
            read)
            .Trim();
    }

    private static string ReadResponseHeaders(
        HttpResponseMessage response)
    {
        const int maxLen = 1024;

        var sb =
            new System.Text.StringBuilder();

        foreach (var h in
                 response.Headers.Concat(
                     response.Content.Headers))
        {
            var line =
                $"{h.Key}: {string.Join(", ", h.Value)}";

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