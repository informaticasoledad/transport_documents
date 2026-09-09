using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.EliminarConductor;

public sealed record EliminarConductorCommand(
    Guid ConductorId)
    : IRequest<ErrorOr<Deleted>>;