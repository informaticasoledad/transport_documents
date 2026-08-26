namespace Dtd.Infrastructure.Configuration;

/// <summary>
/// Options for the ERP microservice gateway. <see cref="UseMock"/> selects the in-memory
/// implementation (deterministic data) instead of the real HTTP adapter. These options hold the
/// <b>shared</b> part of the ERP connection — the OAuth2 client config, which is a single client for
/// every company: <see cref="TokenEndpoint"/>, <see cref="ClientId"/>, <see cref="Scope"/>, the
/// request <see cref="TimeoutSeconds"/> and the (decrypted at startup) <see cref="ClientSecret"/>. The
/// only per-company piece is the base URL of its ERP instance, resolved at runtime from the
/// <c>empresas</c> table.
/// <para>The <c>client_secret</c> is the <b>same for every company</b>; it is <b>not</b> in the
/// <c>empresas</c> table. It is decrypted at startup by <see cref="ErpAuthPostConfigure"/> from
/// <c>Erp:ClientSecret_Enc</c> (AES-256-GCM ciphertext, committable) using the master key injected
/// via <c>ERPAUTH_MASTER_KEY</c>/<c>ERPAUTH_MASTER_KEY_FILE</c> (never committed). Never bound from
/// plaintext config; never logged.</para>
/// <para><see cref="TokenEndpoint"/>, <see cref="ClientId"/> and <see cref="Scope"/> are bound from
/// plaintext <c>appsettings.json</c> (they are not secret). They are validated at call time (in the
/// token provider) rather than with <c>[Required]</c> so <see cref="UseMock"/>=true can run without
/// them.</para>
/// </summary>
public sealed class ErpOptions
{
    public bool UseMock { get; set; } = true;

    /// <summary>Shared OAuth2 <c>token_endpoint</c> of the ERP IdP (client-credentials grant). Same for
    /// every company. Not secret; bound from <c>Erp:TokenEndpoint</c>.</summary>
    public string? TokenEndpoint { get; set; }

    /// <summary>Shared OAuth2 <c>client_id</c> of the ERP client. Same for every company (the
    /// <c>client_secret</c> is shared too, so the client is shared). Not secret; bound from
    /// <c>Erp:ClientId</c>.</summary>
    public string? ClientId { get; set; }

    /// <summary>Shared OAuth2 <c>scope</c> requested by the ERP client, if the IdP requires one. Null
    /// otherwise. Not secret; bound from <c>Erp:Scope</c>.</summary>
    public string? Scope { get; set; }

    /// <summary>Request timeout (seconds) for ERP calls, shared by all enterprises (there is no
    /// per-enterprise override anymore). Defaults to 30.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>How long the per-company endpoint config is cached before re-reading the table.</summary>
    public int EndpointCacheMinutes { get; set; } = 2;

    /// <summary>Seconds subtracted from the OAuth2 token <c>expires_in</c> to refresh before it
    /// actually expires (avoids calling the ERP with an expired token).</summary>
    public int TokenSkewSeconds { get; set; } = 60;

    /// <summary>Fallback token cache TTL (seconds) when the token endpoint does not return
    /// <c>expires_in</c>.</summary>
    public int TokenDefaultTtlSeconds { get; set; } = 300;

    /// <summary>Shared OAuth2 <c>client_secret</c> for the ERP (same for every company). Populated at
    /// startup by <see cref="ErpAuthPostConfigure"/> from <c>Erp:ClientSecret_Enc</c> (AES-256-GCM),
    /// using the master key injected via <c>ERPAUTH_MASTER_KEY</c>. Never bound from plaintext config;
    /// never logged.</summary>
    public string? ClientSecret { get; set; }
}