namespace Dtd.Domain.Ccs;

public readonly record struct CcVinculoAlmacenAgencia(
    Guid AlmacenId,
    Guid AgenciaId,
    bool PorDefecto);

/// <summary>Repository port for the <see cref="Cc"/> reference aggregate.</summary>
public interface ICcRepository
{
    Task<Cc?> GetByIdAsync(Guid ccId, CancellationToken cancellationToken = default);

    Task<Cc?> GetByEmpresaYCodigoAsync(string empresa, string codigo, CancellationToken cancellationToken = default);

    /// <summary>Todos los CCs de una empresa (vista de gestion: activos e inactivos).</summary>
    Task<IReadOnlyList<Cc>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default);

    /// <summary>CCs activos disponibles en alguna relacion del almacen.</summary>
    Task<IReadOnlyList<Cc>> ListarPorAlmacenAsync(Guid almacenId, CancellationToken cancellationToken = default);

    /// <summary>CCs activos disponibles en alguna relacion de la agencia.</summary>
    Task<IReadOnlyList<Cc>> ListarPorAgenciaAsync(Guid agenciaId, CancellationToken cancellationToken = default);

    /// <summary>Devuelve el CC si existe y esta disponible para la relacion almacen-agencia.</summary>
    Task<Cc?> GetByAlmacenYAgenciaEIdAsync(
        Guid almacenId, Guid agenciaId, Guid ccId, CancellationToken cancellationToken = default);

    /// <summary>CCs activos marcados por defecto para la relacion almacen-agencia.</summary>
    Task<IReadOnlyList<Cc>> ObtenerCcsDefectoAsync(
        string empresa, string almacenCodigo, string agenciaCodigo, CancellationToken cancellationToken = default);

    /// <summary>Persiste un CC del catalogo y sus vinculos iniciales con relaciones almacen-agencia.</summary>
    Task AddAsync(
        Cc cc,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken = default);

    /// <summary>Actualiza los vinculos de un CC con relaciones almacen-agencia.</summary>
    Task ActualizarAsync(
        Cc cc,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken = default);

    /// <summary>Marca como por defecto los CCs indicados dentro de la relacion almacen-agencia.</summary>
    Task SetDefectosAsync(
        string empresa, string almacenCodigo, string agenciaCodigo,
        IReadOnlyCollection<Guid> ccIds, CancellationToken cancellationToken = default);
}
