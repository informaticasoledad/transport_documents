using Dtd.Domain.Agencias;

namespace Dtd.Domain.Almacenes;

/// <summary>Repository port for the <see cref="Almacen"/> reference aggregate.</summary>
public interface IAlmacenRepository
{
    /// <summary>Busca un almacén por su <c>Id</c>. No filtra por <c>Activo</c>: la validación al
    /// generar/enviar usa la existencia, no el estado (un almacén desactivado tras generar no debe
    /// romper <c>EnviarDocumentoADocuten</c>/<c>ObtenerDocumento</c>).</summary>
    Task<Almacen?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Resuelve varios almacenes por <c>Id</c> en una sola consulta (enriquecimiento del
    /// read model de documentos: ID + código + nombre). No filtra por <c>Activo</c>.</summary>
    Task<IReadOnlyList<Almacen>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Busca un almacén por empresa + código (clave natural). No filtra por <c>Activo</c>.</summary>
    Task<Almacen?> GetByEmpresaYCodigoAsync(string empresa, string codigo, CancellationToken cancellationToken = default);

    /// <summary>Almacenes activos de una empresa (para el dropdown de selección del front).</summary>
    Task<IReadOnlyList<Almacen>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default);

    /// <summary>Agencias (carriers) disponibles para un almacén (unión <c>almacen_agencias</c>).</summary>
    Task<IReadOnlyList<Agencia>> ListarAgenciasDisponiblesAsync(string empresa, string codigo, CancellationToken cancellationToken = default);

    /// <summary>True si la agencia está entre las disponibles del almacén (validación al generar).</summary>
    Task<bool> EsAgenciaDisponibleAsync(Guid almacenId, Guid agenciaId, CancellationToken cancellationToken = default);

    Task<AlmacenAgencia?> GetRelacionAgenciaAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    Task<AlmacenAgencia?> GetRelacionAgenciaParaActualizarAsync(
        Guid almacenId,
        Guid agenciaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Almacen almacen, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Almacen>> ObtenerPorCodigosAsync(
    string empresa,
    IReadOnlyCollection<string> codigos,
    CancellationToken cancellationToken);
}
