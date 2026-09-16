using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.CrearBaseAgencia;

public sealed record CrearBaseAgenciaCommand(
    Guid AgenciaId,
    string Codigo,
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string Language,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso)
    : IRequest<ErrorOr<AgenciaBaseDto>>;