namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Resuelve la configuración del endpoint del microservicio ERP de una empresa dada, con una caché
/// en memoria de corta duración para no golpear la base de datos en cada consulta de expediciones.
/// Devuelve <c>null</c> cuando la empresa no tiene endpoint configurado (el gateway lanza entonces
/// <see cref="EmpresaNoConfiguradaException"/>).
/// </summary>
public interface IEmpresaResolver
{
    Task<EmpresaConfig?> ResolveAsync(string empresa, CancellationToken cancellationToken = default);
}