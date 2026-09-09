namespace Dtd.Domain.Agencias;

/// <summary>
/// Repository port para el agregado de referencia <see cref="Agencia"/>.
/// Las agencias son globales y se relacionan con los almacenes mediante
/// la configuración correspondiente.
/// </summary>
public interface IAgenciaRepository
{
    /// <summary>
    /// Busca una agencia por su <c>Id</c>.
    /// No filtra por <c>Activa</c>.
    /// </summary>
    Task<Agencia?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca una agencia por su código global.
    /// No filtra por <c>Activa</c>.
    /// </summary>
    Task<Agencia?> GetByCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resuelve varias agencias por <c>Id</c> en una sola consulta.
    /// No filtra por <c>Activa</c>.
    /// </summary>
    Task<IReadOnlyList<Agencia>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todas las agencias.
    /// </summary>
    Task<IReadOnlyList<Agencia>> ListarAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera una agencia junto con sus bases.
    /// Se utiliza para operaciones que modifican el agregado completo.
    /// </summary>
    Task<Agencia?> GetByIdConBasesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade una nueva agencia.
    /// </summary>
    Task AddAsync(
        Agencia agencia,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///  Borrar Agencia
    /// </summary>
    void Remove(Agencia agencia);

    Task<IReadOnlyList<Agencia>> ListarActivasAsync(CancellationToken cancellationToken);


    Task<(IReadOnlyList<Agencia> Items, int Total)> BuscarAsync(
        string? texto,
        bool? activa,
        bool? envioDirecto,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}