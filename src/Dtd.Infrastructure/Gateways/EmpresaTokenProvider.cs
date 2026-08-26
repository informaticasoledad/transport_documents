using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dtd.Application.GatewayContracts;
using Dtd.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure.Gateways;

/// <summary>
/// <see cref="IEmpresaTokenProvider"/> respaldado por una llamada HTTP al token endpoint OAuth2
/// (grant client-credentials) del ERP. El access token se cachea por <c>empresa</c> con un pequeño
/// skew (<see cref="ErpOptions.TokenSkewSeconds"/>) para refrescarlo antes de que expire de verdad.
/// Cuando el token endpoint no devuelve <c>expires_in</c>, se usa un TTL por defecto conservador
/// (<see cref="ErpOptions.TokenDefaultTtlSeconds"/>).
/// <para>Toda la configuración del cliente OAuth2 es común a todas las empresas (un único cliente
/// ERP): el <c>token_endpoint</c>, el <c>client_id</c>, el <c>scope</c> y el <c>client_secret</c>
/// descifrado vienen de <see cref="ErpOptions"/> (appsettings). El <paramref name="config"/> por
/// empresa solo lleva la URL base de la empresa; se pasa igualmente para keyear la caché de token
/// por <c>empresa</c>.</para>
/// </summary>
internal sealed class EmpresaTokenProvider : IEmpresaTokenProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ErpOptions _options;

    public EmpresaTokenProvider(IHttpClientFactory httpClientFactory, IMemoryCache cache, IOptions<ErpOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<string> GetTokenAsync(EmpresaConfig config, CancellationToken cancellationToken = default)
    {
        // El cliente OAuth2 del ERP es COMÚN a todas las empresas (token_endpoint, client_id, scope y
        // client_secret compartidos, todos en appsettings/ErpOptions). Si falta cualquiera de ellos,
        // es un error de configuración de ops: se reporta como "ERP no configurado" (500) sin revelar
        // el secret. La empresa (config) solo aporta su base_address; aquí la usamos para keyear la
        // caché de token por empresa.
        if (string.IsNullOrWhiteSpace(_options.TokenEndpoint))
        {
            throw new EmpresaNoConfiguradaException(
                config.Empresa,
                $"Falta el token_endpoint del ERP: debe ir (en claro, no es secreto) en " +
                $"'Erp:TokenEndpoint' (appsettings). El cliente OAuth2 es común a todas las empresas.");
        }
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new EmpresaNoConfiguradaException(
                config.Empresa,
                $"Falta el client_id del ERP: debe ir (en claro, no es secreto) en 'Erp:ClientId' " +
                $"(appsettings). El cliente OAuth2 es común a todas las empresas.");
        }
        if (string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new EmpresaNoConfiguradaException(
                config.Empresa,
                $"Falta el client_secret del ERP: debe ir cifrado en 'Erp:ClientSecret_Enc' " +
                $"(con la master key inyectada por ERPAUTH_MASTER_KEY / ERPAUTH_MASTER_KEY_FILE). " +
                $"Revisa que el bloque ClientSecret_Enc exista en appsettings y que ERPAUTH_MASTER_KEY " +
                $"esté disponible (env / user-secrets / Secret de k8s).");
        }

        var token = await _cache.GetOrCreateAsync(CacheKey(config.Empresa), async entry =>
        {
            var token = await RequestTokenAsync(cancellationToken).ConfigureAwait(false);
            // TTL: expires_in menos skew, con mínimo de 10s; fallback al TTL por defecto si es desconocido.
            var ttl = token.ExpiresInSeconds > 0
                ? TimeSpan.FromSeconds(Math.Max(10, token.ExpiresInSeconds - _options.TokenSkewSeconds))
                : TimeSpan.FromSeconds(_options.TokenDefaultTtlSeconds);
            entry.AbsoluteExpirationRelativeToNow = ttl;
            return token.AccessToken;
        });
        return token ?? throw new InvalidOperationException("La caché de token devolvió null inesperadamente.");
    }

    private async Task<TokenResponse> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ErpToken");

        var form = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", _options.ClientId!),
            new("client_secret", _options.ClientSecret!)
        };
        if (!string.IsNullOrWhiteSpace(_options.Scope))
        {
            form.Add(new("scope", _options.Scope!));
        }

        // No registrar jamás el cuerpo del form (lleva el client_secret) ni el access_token.
        using var content = new FormUrlEncodedContent(form);
        using var response = await client.PostAsync(_options.TokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var token = await JsonSerializer.DeserializeAsync<TokenResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta del token endpoint del ERP estaba vacía.");
        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("La respuesta del token endpoint del ERP no contenía access_token.");
        }
        return token;
    }

    private static string CacheKey(string empresa) => $"empresa:token:{empresa}";

    private sealed record TokenResponse
    {
        [property: JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [property: JsonPropertyName("token_type")]
        public string? TokenType { get; init; }

        [property: JsonPropertyName("expires_in")]
        public long ExpiresInSeconds { get; init; }
    }
}