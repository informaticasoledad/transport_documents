using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ListarAgenciaBases;

public sealed record ListarAgenciaBasesQuery(
    Guid AgenciaId)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseDto>>>;