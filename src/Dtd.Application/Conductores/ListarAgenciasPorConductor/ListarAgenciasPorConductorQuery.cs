using ErrorOr;
using MediatR;
using Dtd.Application.Agencias;

namespace Dtd.Application.Conductores.ListarAgenciasPorConductor;

public sealed record ListarAgenciasPorConductorQuery(
    Guid ConductorId)
    : IRequest<ErrorOr<IReadOnlyCollection<AgenciaDto>>>;