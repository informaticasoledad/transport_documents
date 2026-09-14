using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ModificarAlmacenAgencia;

public sealed record ModificarAlmacenAgenciaCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid TemplateId,
    Guid? AgenciaBaseId)
    : IRequest<ErrorOr<AlmacenAgenciaDto>>;