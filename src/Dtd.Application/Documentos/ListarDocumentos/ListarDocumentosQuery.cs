using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarDocumentos;

/// <summary>Lists documents matching the optional filters (all optional except none required).</summary>
public sealed record ListarDocumentosQuery(
    string? Empresa = null,
    string? AlmacenCodigo = null,
    string? AgenciaCodigo = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    string? Estado = null) : IRequest<ErrorOr<IReadOnlyList<DocumentoDto>>>;