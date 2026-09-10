using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAlmacenes;

public sealed record ListarAlmacenesQuery(
    string Empresa,
    string? Texto = null,
    bool? Activo = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<ErrorOr<AlmacenesPaginadosDto>>;