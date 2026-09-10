using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ModificarAlmacen;

public sealed record ModificarAlmacenCommand(
    string Empresa,
    Guid AlmacenId,
    string Nombre,
    string Direccion,
    string CodigoPostal,
    string Ciudad,
    string CodigoPaisIso,
    string? Email,
    string? Telefono,
    string TipoFirmaConsignor,
    bool Activo)
    : IRequest<ErrorOr<AlmacenDto>>;