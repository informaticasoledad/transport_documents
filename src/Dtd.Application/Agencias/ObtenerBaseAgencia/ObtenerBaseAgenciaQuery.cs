using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ObtenerBaseAgencia;

public sealed record ObtenerBaseAgenciaQuery(
    Guid AgenciaId,
    Guid BaseId)
    : IRequest<ErrorOr<AgenciaBaseDto>>;