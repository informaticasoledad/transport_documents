using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductoresCatalogo;

/// <summary>
/// Lista el catálogo global de conductores con filtros y paginación.
/// </summary>
public sealed record ListarConductoresCatalogoQuery(
    string? Texto = null,
    bool? Activo = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<ErrorOr<ConductoresPaginadosDto>>;