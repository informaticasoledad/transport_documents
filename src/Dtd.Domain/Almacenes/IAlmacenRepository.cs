using Dtd.Domain.Agencias;

namespace Dtd.Domain.Almacenes;

/// <summary>
/// Repository port for the <see cref="Almacen"/> reference aggregate.
/// </summary>
public interface IAlmacenRepository
{
    /// <summary>
    /// Busca un almacén por su Id. No filtra por Activo.
    /// </summary>
    Task<Almacen?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resuelve varios almacenes por Id en una sola consulta.
    /// No filtra por Activo.
    /// </summary>
    Task<IReadOnlyList<Almacen>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un almacén por empresa + código.
    /// No filtra por Activo.
    /// </summary>
    Task<Almacen?> GetByEmpresaYCodigoAsync(
        string empresa,
        string codigo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca almacenes de una empresa aplicando permisos, filtros y paginación.
    /// </summary>
    Task<(IReadOnlyList<Almacen> Items, int Total)> BuscarAsync(
        string empresa,
        IReadOnlyCollection<Guid> idsPermitidos,
        string? texto,
        bool? activo,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Almacenes activos de una empresa.
    /// Mantener mientras existan consumidores que necesiten el listado completo.
    /// </summary>
    Task<IReadOnlyList<Almacen>> ListarPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agencias disponibles para un almacén.
    /// </summary>
    Task<IReadOnlyList<Agencia>> ListarAgenciasDisponiblesAsync(
        Guid almacenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si una agencia está disponible para el almacén.
    /// </summary>
    Task<bool> EsAgenciaDisponibleAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    Task<AlmacenAgencia?> GetRelacionAgenciaAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    Task<AlmacenAgencia?> GetRelacionAgenciaParaActualizarAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Almacen almacen,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Almacen>> ObtenerPorCodigosAsync(
        string empresa,
        IReadOnlyCollection<string> codigos,
        CancellationToken cancellationToken);
}