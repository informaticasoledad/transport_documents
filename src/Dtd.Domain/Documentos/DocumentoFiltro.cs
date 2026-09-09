namespace Dtd.Domain.Documentos;

public sealed record DocumentoFiltro(
    string? Empresa = null,
    IReadOnlyCollection<string>? Empresas = null,
    Guid? AlmacenId = null,
    Guid? AgenciaId = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    EstadoDocumento? Estado = null,
    bool? Finalizado = null);

