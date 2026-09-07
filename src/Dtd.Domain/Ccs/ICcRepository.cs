namespace Dtd.Domain.Ccs;

public readonly record struct CcVinculoAlmacenAgencia(
    Guid AlmacenId,
    Guid AgenciaId,
    bool PorDefecto);

/// <summary>
/// Repository port for the <see cref="Cc"/> reference aggregate.
/// </summary>
public interface ICcRepository
{
    Task<Cc?> GetByIdAsync(
        Guid ccId,
        CancellationToken cancellationToken = default);

    Task<Cc?> GetByEmpresaYCodigoAsync(
        string empresa,
        string codigo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Todos los CCs de una empresa
    /// (vista de gestión: activos e inactivos).
    /// </summary>
    Task<IReadOnlyList<Cc>> ListarPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CCs activos disponibles en alguna relación del almacén.
    /// </summary>
    Task<IReadOnlyList<Cc>> ListarPorAlmacenAsync(
        Guid almacenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CCs activos disponibles en alguna relación de la agencia.
    /// </summary>
    Task<IReadOnlyList<Cc>> ListarPorAgenciaAsync(
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve el CC si existe y está disponible para la relación almacén-agencia.
    /// </summary>
    Task<Cc?> GetByAlmacenYAgenciaEIdAsync(
        Guid almacenId,
        Guid agenciaId,
        Guid ccId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CCs activos marcados por defecto para la relación almacén-agencia.
    /// </summary>
    Task<IReadOnlyList<Cc>> ObtenerCcsDefectoAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste un CC del catálogo y sus vínculos iniciales
    /// con relaciones almacén-agencia.
    /// </summary>
    Task AddAsync(
        Cc cc,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los vínculos de un CC con relaciones almacén-agencia.
    /// </summary>
    Task ActualizarAsync(
        Cc cc,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca como por defecto los CCs indicados
    /// dentro de la relación almacén-agencia.
    /// </summary>
    Task SetDefectosAsync(
        Guid almacenId,
        Guid agenciaId,
        IReadOnlyCollection<Guid> ccIds,
        CancellationToken cancellationToken = default);
}