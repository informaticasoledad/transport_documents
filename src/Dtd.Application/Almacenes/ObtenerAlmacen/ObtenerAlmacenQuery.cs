using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ObtenerAlmacen;

public sealed record ObtenerAlmacenQuery(
    string Empresa,
    Guid AlmacenId)
    : IRequest<ErrorOr<AlmacenDto>>;