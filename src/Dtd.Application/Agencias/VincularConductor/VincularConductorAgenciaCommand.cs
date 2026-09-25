using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.VincularConductor;

public sealed record VincularConductorAgenciaCommand(
    Guid AgenciaId,
    Guid ConductorId)
    : IRequest<ErrorOr<Success>>;