using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EliminarConductorDefecto;

public sealed record EliminarConductorDefectoCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid ConductorId)
    : IRequest<ErrorOr<Success>>;