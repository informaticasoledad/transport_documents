namespace Dtd.Domain.AgenciaBases;

public interface IAgenciaBaseRepository
{
    Task<AgenciaBase?> GetByIdAsync(Guid agenciaBaseId, CancellationToken cancellationToken = default);

    Task<AgenciaBase?> GetByEmpresaYCodigoAsync(
        string empresa,
        string codigo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgenciaBase>> ListarPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgenciaBase>> ListarActivosPorEmpresaAsync(
        string empresa,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgenciaBase>> ObtenerAgenciaBasesDefectoAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AgenciaBase agenciaBase,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        AgenciaBase agenciaBase,
        CancellationToken cancellationToken = default);

    Task SetDefectosAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        IReadOnlyCollection<Guid> agenciaBaseIds,
        CancellationToken cancellationToken = default);
}
