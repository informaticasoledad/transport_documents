using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.EliminarAgencia;

public sealed record EliminarAgenciaCommand(
    Guid AgenciaId)
    : IRequest<ErrorOr<Deleted>>;