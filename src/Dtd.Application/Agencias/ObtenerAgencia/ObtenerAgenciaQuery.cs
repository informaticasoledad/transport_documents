using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ObtenerAgencia;

public sealed record ObtenerAgenciaQuery(
    Guid AgenciaId)
    : IRequest<ErrorOr<AgenciaDto>>;