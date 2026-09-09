using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ModificarConductor;

public sealed record ModificarConductorCommand(
    Guid ConductorId,
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string? LicensePlate,
    string Language,
    bool Activo)
    : IRequest<ErrorOr<ConductorCatalogoDto>>;