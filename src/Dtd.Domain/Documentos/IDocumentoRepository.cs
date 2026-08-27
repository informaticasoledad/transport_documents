using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos;

/// <summary>
/// Repository port for the <see cref="DocumentoDigitalTransporte"/> aggregate.
/// Implemented in the infrastructure layer (EF Core).
/// </summary>
public interface IDocumentoRepository
{
    Task<DocumentoDigitalTransporte?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(DocumentoDigitalTransporte documento, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subset of the given ERP expedition ids that are already persisted
    /// (i.e. already included in some document). Used to compute "expediciones no incluidas todavía".
    /// </summary>
    Task<IReadOnlySet<string>> ObtenerErpIdsIncluidosAsync(
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        IReadOnlyCollection<string> erpIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoDigitalTransporte>> ListarAsync(
        DocumentoFiltro filtro,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-side filters for listing documents. <c>Empresa</c> es un filtro explícito (una empresa);
/// <c>Empresas</c> restringe el listado a un conjunto (las empresas autorizadas del usuario, ya
/// autorizadas). Ambos son opcionales y mutuamente compatibles (se aplican en AND).
/// </summary>
public sealed record DocumentoFiltro(
    string? Empresa = null,
    IReadOnlyCollection<string>? Empresas = null,
    Guid? AlmacenId = null,
    Guid? AgenciaId = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    EstadoDocumento? Estado = null,
    bool? Finalizado = null);
