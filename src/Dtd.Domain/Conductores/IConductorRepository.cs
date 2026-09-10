namespace Dtd.Domain.Conductores;

/// <summary>
/// Repository port para el agregado de referencia <see cref="Conductor"/>.
/// Los conductores forman un catálogo global y pueden estar vinculados
/// M:N con agencias mediante <c>conductor_agencias</c>.
/// </summary>
public interface IConductorRepository
{
    /// <summary>
    /// Obtiene un conductor del catálogo por Id.
    /// </summary>
    Task<Conductor?> GetByIdAsync(
        Guid conductorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve el conductor si existe y está vinculado a la agencia indicada
    /// mediante <c>conductor_agencias</c>.
    /// Devuelve <c>null</c> si no existe o no está vinculado a esa agencia.
    /// No filtra por <c>Activo</c>.
    /// </summary>
    Task<Conductor?> GetByAgenciaYIdAsync(
        Guid agenciaId,
        Guid conductorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los conductores activos vinculados a una agencia.
    /// </summary>
    Task<IReadOnlyList<Conductor>> ListarPorAgenciaAsync(
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca conductores del catálogo global con filtros y paginación.
    /// </summary>
    Task<(IReadOnlyList<Conductor> Items, int Total)> BuscarAsync(
        string? texto,
        bool? activo,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los conductores por defecto configurados para una relación
    /// almacén-agencia.
    /// Devuelve únicamente conductores activos y vinculados a la agencia.
    /// </summary>
    Task<IReadOnlyList<Conductor>> ObtenerConductoresDefectoAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste un conductor del catálogo.
    /// No crea vínculos con agencias.
    /// </summary>
    Task AddAsync(
        Conductor conductor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un conductor del catálogo.
    /// </summary>
    void Remove(Conductor conductor);

    Task<bool> ExistsByTaxIdAsync(
    string taxId,
    CancellationToken cancellationToken = default);

    Task<bool> ExistsByTaxIdExceptIdAsync(
    string taxId,
    Guid conductorId,
    CancellationToken cancellationToken = default);

}