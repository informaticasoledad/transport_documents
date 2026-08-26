namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Puerto de lectura de la configuración del endpoint ERP por empresa (tabla <c>empresas</c>).
/// Implementado en la capa de infraestructura; consumido por <see cref="IEmpresaResolver"/>.
/// </summary>
public interface IEmpresaRepository
{
    /// <summary>Devuelve la configuración del endpoint de la empresa, o <c>null</c> si no está configurada.</summary>
    Task<EmpresaConfig?> GetByEmpresaAsync(string empresa, CancellationToken cancellationToken = default);
}