using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.AgregarConductorDefecto;

public sealed record AgregarConductorDefectoCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid ConductorId)
    : IRequest<ErrorOr<Success>>;