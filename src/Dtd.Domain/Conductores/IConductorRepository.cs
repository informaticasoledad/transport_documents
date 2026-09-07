namespace Dtd.Domain.Conductores;

/// <summary>
/// Repository port for the <see cref="Conductor"/> reference aggregate
/// (catálogo por empresa, vinculado M:N a agencias vía <c>conductor_agencias</c>).
/// </summary>
public interface IConductorRepository
{
    /// <summary>
    /// Devuelve el conductor si existe Y está vinculado a la agencia dada
    /// (join <c>conductor_agencias</c>), activo o no.
    /// <c>null</c> si no existe o no está vinculado a esa agencia.
    /// </summary>
    Task<Conductor?> GetByAgenciaYIdAsync(
        Guid agenciaId,
        Guid conductorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conductor>> ListarPorAgenciaAsync(
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conductores por defecto de la tupla (empresa, almacén, agencia):
    /// lee <c>almacen_agencia_conductores_defecto</c> y resuelve el catálogo
    /// filtrando por activo y por vínculo con esa agencia.
    /// Lista vacía si no hay defaults.
    /// </summary>
    Task<IReadOnlyList<Conductor>> ObtenerConductoresDefectoAsync(
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste un conductor del catálogo y sus vínculos iniciales con agencias.
    /// </summary>
    Task AddAsync(
        Conductor conductor,
        IReadOnlyCollection<Guid> agenciaIds,
        CancellationToken cancellationToken = default);
}