using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.GenerarDocumento;

public sealed record GenerarDocumentoCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    DateOnly FechaDesde,
    DateOnly FechaHasta) : IRequest<ErrorOr<Guid>>;