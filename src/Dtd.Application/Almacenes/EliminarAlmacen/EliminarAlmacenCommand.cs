using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EliminarAlmacen;

public sealed record EliminarAlmacenCommand(
    string Empresa,
    Guid AlmacenId)
    : IRequest<ErrorOr<Deleted>>;