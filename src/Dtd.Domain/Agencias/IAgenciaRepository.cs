namespace Dtd.Domain.Agencias;

/// <summary>Repository port for the <see cref="Agencia"/> reference aggregate (per-empresa).</summary>
public interface IAgenciaRepository
{
    /// <summary>Busca una agencia por su <c>Id</c>. No filtra por <c>Activa</c>: la validación al
    /// generar/enviar usa la existencia, no el estado.</summary>
    Task<Agencia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Resuelve varias agencias por <c>Id</c> en una sola consulta (enriquecimiento del
    /// read model de documentos). No filtra por <c>Activa</c>.</summary>
    Task<IReadOnlyList<Agencia>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<Agencia?> GetByEmpresaYCodigoAsync(string empresa, string codigo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Agencia>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default);

    Task AddAsync(Agencia agencia, CancellationToken cancellationToken = default);
}