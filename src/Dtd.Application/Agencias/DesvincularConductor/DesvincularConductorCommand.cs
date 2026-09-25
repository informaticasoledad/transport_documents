using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.DesvincularConductor;

public sealed record DesvincularConductorAgenciaCommand(
    Guid AgenciaId,
    Guid ConductorId)
    : IRequest<ErrorOr<Success>>;