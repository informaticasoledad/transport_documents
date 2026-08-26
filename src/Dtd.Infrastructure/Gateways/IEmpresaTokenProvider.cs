using Dtd.Application.GatewayContracts;

namespace Dtd.Infrastructure.Gateways;

/// <summary>
/// Obtiene un JWT bearer token para el ERP de una empresa vía el grant OAuth2 client-credentials,
/// cacheándolo por <c>empresa</c> hasta poco antes de que expire. Concern de infraestructura: la
/// capa de aplicación no conoce la adquisición de tokens.
/// </summary>
internal interface IEmpresaTokenProvider
{
    /// <summary>Devuelve un access token no expirado para el ERP de la empresa.</summary>
    Task<string> GetTokenAsync(EmpresaConfig config, CancellationToken cancellationToken = default);
}