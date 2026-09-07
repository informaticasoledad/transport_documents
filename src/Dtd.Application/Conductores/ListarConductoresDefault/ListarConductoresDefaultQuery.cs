using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductoresDefault;

public sealed record ListarConductoresDefaultQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId)
    : IRequest<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>;