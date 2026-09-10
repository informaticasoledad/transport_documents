using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.CrearAlmacen;

public sealed record CrearAlmacenCommand(
    string Empresa,
    string Codigo,
    string Nombre,
    string Direccion,
    string CodigoPostal,
    string Ciudad,
    string CodigoPaisIso,
    string? Email,
    string? Telefono,
    string TipoFirmaConsignor)
    : IRequest<ErrorOr<AlmacenDto>>;