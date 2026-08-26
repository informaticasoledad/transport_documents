namespace Dtd.Infrastructure.Configuration;

/// <summary>
/// Opciones de OIDC contra Keycloak para el back del DDT (resource server JWT).
/// Solo se aplican cuando <c>Auth:Enabled=true</c> (leído directamente de configuración en
/// <c>Program.cs</c>, igual que <c>Database:AutoApplyMigrations</c>). <see cref="Authority"/> y
/// <see cref="Audience"/> <b>no</b> llevan <c>[Required]</c> a propósito: con Auth deshabilitada
/// (dev) pueden ir vacíos y <c>ValidateOnStart</c> no debe abortar el arranque.
/// </summary>
public sealed class KeycloakOptions
{
    /// <summary>URL del realm de Keycloak (descubre JWKS en /.well-known/openid-configuration).</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>Audience/client al que va dirigido el token (ValidAudience).</summary>
    public string Audience { get; set; } = "dtd-api";

    /// <summary>Nombre del claim que lleva las empresas autorizadas del usuario.</summary>
    public string EmpresasClaimType { get; set; } = "empresas";

    /// <summary>Claim del nombre a efectos de auditoría/display.</summary>
    public string NameClaimType { get; set; } = "preferred_username";

    /// <summary>Require HTTPS para la metadata de Keycloak (false solo en dev con http).</summary>
    public bool RequireHttpsMetadata { get; set; } = true;
}