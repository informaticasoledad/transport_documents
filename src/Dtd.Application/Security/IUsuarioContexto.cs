namespace Dtd.Application.Security;

/// <summary>
/// Puerto de acceso al usuario autenticado actual y a las empresas que tiene autorizadas.
/// La implementación HTTP vive en el Api y lee los claims del token OIDC/Keycloak.
/// Cuando la autenticación está deshabilitada (Auth:Enabled=false, dev), <see cref="Current"/>
/// es <c>null</c> y los handlers omiten el chequeo de empresa (flujo anónimo de desarrollo).
/// </summary>
public interface IUsuarioContexto
{
    /// <summary>Info del usuario autenticado, o null si no hay token/autenticación deshabilitada.</summary>
    UsuarioInfo? Current { get; }
}