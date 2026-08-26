namespace Dtd.Domain.Conductores;

/// <summary>Repository port for the <see cref="Conductor"/> reference aggregate (catálogo por empresa,
/// vinculado M:N a agencias vía <c>conductor_agencias</c>).</summary>
public interface IConductorRepository
{
    /// <summary>Devuelve el conductor si existe Y está vinculado a la agencia dada (join
    /// <c>conductor_agencias</c>), activo o no (el caller distingue 404 vs <c>Inactivo</c>).
    /// <c>null</c> si no existe o no está vinculado a esa agencia.</summary>
    Task<Conductor?> GetByAgenciaYIdAsync(Guid agenciaId, Guid conductorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conductor>> ListarPorAgenciaAsync(Guid agenciaId, CancellationToken cancellationToken = default);

    /// <summary>Conductores por defecto de la tupla (empresa, almacén, agencia): lee
    /// <c>almacen_agencia_conductores_defecto</c> y resuelve el catálogo filtrando por activo Y por
    /// vínculo con esa agencia (un default que apunta a un conductor desvinculado se excluye).
    /// Lista vacía si no hay defaults o el almacén/agencia no existen. El back no los auto-adjunta;
    /// los consume el endpoint dedicado para que el front los añada.</summary>
    Task<IReadOnlyList<Conductor>> ObtenerConductoresDefectoAsync(
        string empresa, string almacenCodigo, string agenciaCodigo, CancellationToken cancellationToken = default);

    /// <summary>Persiste un conductor del catálogo y sus vínculos iniciales con agencias
    /// (filas de <c>conductor_agencias</c>). No hace <c>SaveChanges</c> (lo hace el <c>IUnitOfWork</c>).
    /// Base para el maestro CRUD futuro y para seeds/tests.</summary>
    Task AddAsync(Conductor conductor, IReadOnlyCollection<Guid> agenciaIds, CancellationToken cancellationToken = default);
}