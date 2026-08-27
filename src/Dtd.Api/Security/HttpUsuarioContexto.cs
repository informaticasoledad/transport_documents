using System.Security.Claims;
using Dtd.Application.Security;
using Dtd.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Dtd.Api.Security;

/// <summary>
/// Implementación HTTP de <see cref="IUsuarioContexto"/>:
/// obtiene la identidad del usuario autenticado desde los claims OIDC/Keycloak.
/// </summary>
internal sealed class HttpUsuarioContexto : IUsuarioContexto
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly KeycloakOptions _keycloak;

    public HttpUsuarioContexto(
        IHttpContextAccessor httpContextAccessor,
        IOptions<KeycloakOptions> keycloak)
    {
        _httpContextAccessor = httpContextAccessor;
        _keycloak = keycloak.Value;
    }

    public UsuarioActual? Current
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user is null ||
                user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var id =
                user.FindFirst("sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var username =
                user.FindFirst(_keycloak.NameClaimType)?.Value
                ?? user.Identity?.Name;

            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return new UsuarioActual(
                id,
                username);
        }
    }
}