using Dtd.Application.Agencias;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.VincularAlmacenAgencia;

public sealed record VincularAlmacenAgenciaCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid TemplateId,
    Guid? AgenciaBaseId)
    : IRequest<ErrorOr<AlmacenAgenciaDto>>;