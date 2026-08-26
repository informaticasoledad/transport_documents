using System.Security.Claims;
using System.Text.Json;
using Dtd.Application.Security;
using Dtd.Domain.Empresas;
using Dtd.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Dtd.Api.Security;

/// <summary>
/// Implementacion HTTP de <see cref="IUsuarioContexto"/>: lee la identidad y las empresas autorizadas
/// de los claims del token OIDC/Keycloak.
/// </summary>
internal sealed class HttpUsuarioContexto : IUsuarioContexto
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly KeycloakOptions _keycloak;

    public HttpUsuarioContexto(IHttpContextAccessor httpContextAccessor, IOptions<KeycloakOptions> keycloak)
    {
        _httpContextAccessor = httpContextAccessor;
        _keycloak = keycloak.Value;
    }

    public UsuarioInfo? Current
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null || user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var sub = user.FindFirst("sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
            var nombre = user.FindFirst(_keycloak.NameClaimType)?.Value ?? user.Identity?.Name;
            var empresas = LeerEmpresas(user);

            return new UsuarioInfo(sub, nombre, empresas);
        }
    }

    private IReadOnlySet<string> LeerEmpresas(ClaimsPrincipal user)
    {
        var claims = user.FindAll(_keycloak.EmpresasClaimType).Select(c => c.Value).ToList();
        if (claims.Count == 0)
        {
            return new HashSet<string>();
        }

        var valores = new List<string>();
        foreach (var valor in claims)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                continue;
            }

            var trimmed = valor.Trim();
            if (trimmed.StartsWith('['))
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<List<string>>(trimmed);
                    if (arr is not null)
                    {
                        valores.AddRange(arr.Where(s => !string.IsNullOrWhiteSpace(s)));
                    }
                }
                catch (JsonException)
                {
                    valores.Add(trimmed);
                }
            }
            else if (trimmed.Contains(','))
            {
                valores.AddRange(trimmed.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                valores.Add(trimmed);
            }
        }

        var empresas = new HashSet<string>(StringComparer.Ordinal);
        foreach (var valor in valores)
        {
            if (Empresa.EsValida(valor))
            {
                empresas.Add(valor.Trim());
            }
        }

        return empresas;
    }
}
