using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.EliminarBaseAgencia;

public sealed record EliminarBaseAgenciaCommand(
    Guid AgenciaId,
    Guid BaseId)
    : IRequest<ErrorOr<Deleted>>;