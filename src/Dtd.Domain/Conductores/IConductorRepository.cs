namespace Dtd.Domain.Conductores;

/// <summary>
/// Repository port para el agregado de referencia <see cref="Conductor"/>.
/// Los conductores forman un catálogo global y pueden estar vinculados
/// M:N con agencias mediante <c>conductor_agencias</c>.
/// </summary>
public interface IConductorRepository
{
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
    /// Obtiene los conductores por defecto configurados para una relación
    /// almacén-agencia.
    /// Lee <c>almacen_agencia_conductores_defecto</c> y devuelve únicamente
    /// conductores activos y vinculados a la agencia.
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
}