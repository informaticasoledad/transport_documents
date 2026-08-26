namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Configuración de conexión con el microservicio ERP de una empresa dada. Se resuelve en runtime por
/// <c>empresa</c> desde la tabla <c>empresas</c>; lleva solo la parte por empresa y no sensible:
/// <c>BaseAddress</c> (cada empresa tiene su propia URL de instancia de ERP).
/// <para>El resto de la conexión con el ERP es <b>común a todas las empresas</b> (cliente OAuth2 único:
/// token endpoint, client id, scope y el client_secret) y vive a nivel de app en <c>ErpOptions</c>
/// (<c>appsettings.json</c>). El <c>client_secret</c> en particular se descifra al arrancar desde
/// <c>Erp:ClientSecret_Enc</c> (ver <c>ErpOptions.ClientSecret</c>). La BD no guarda nada de eso.</para>
/// <para>El ERP se autentica con un JWT bearer token obtenido vía el grant OAuth2 client-credentials;
/// la cabecera de API key ya no se usa.</para>
/// </summary>
public sealed record EmpresaConfig(
    string Empresa,
    string BaseAddress,
    string? TaxId = null,
    string? Nombre = null);