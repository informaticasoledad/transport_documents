using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ModificarBaseAgencia;

public sealed record ModificarBaseAgenciaCommand(
    Guid AgenciaId,
    Guid BaseId,
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