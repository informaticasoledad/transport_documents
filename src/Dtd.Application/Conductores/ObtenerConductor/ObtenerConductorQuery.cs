using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ObtenerConductor;

public sealed record ObtenerConductorQuery(
    Guid ConductorId)
    : IRequest<ErrorOr<ConductorCatalogoDto>>;