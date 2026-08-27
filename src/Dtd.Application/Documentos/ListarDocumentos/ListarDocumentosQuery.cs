using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarDocumentos;

/// <summary>Lists documents matching the optional filters (all optional except none required).</summary>
public sealed record ListarDocumentosQuery(
    string Empresa = null!,
    Guid? AlmacenId = null,
    Guid? AgenciaId = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    string? Estado = null,
    bool? Finalizado = null) : IRequest<ErrorOr<IReadOnlyList<DocumentoDto>>>;