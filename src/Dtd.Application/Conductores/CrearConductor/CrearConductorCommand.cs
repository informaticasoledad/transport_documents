using Dtd.Application.Conductores;
using ErrorOr;
using MediatR;

public sealed record CrearConductorCommand(
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string? LicensePlate,
    string Language)
    : IRequest<ErrorOr<ConductorCatalogoDto>>;