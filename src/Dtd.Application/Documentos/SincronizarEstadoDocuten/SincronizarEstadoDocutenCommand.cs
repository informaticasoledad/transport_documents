using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.SincronizarEstadoDocuten;

/// <summary>
/// Polls Docuten for the current status of a previously transmitted document and updates the
/// aggregate accordingly. Will be replaced by a webhook receiver once Docuten provides it.
/// </summary>
public sealed record SincronizarEstadoDocutenCommand(Guid DocumentoId) : IRequest<ErrorOr<DocumentoEstadoDto>>;

public sealed record DocumentoEstadoDto(Guid DocumentoId, string Estado, string? PlataformaEstado);
