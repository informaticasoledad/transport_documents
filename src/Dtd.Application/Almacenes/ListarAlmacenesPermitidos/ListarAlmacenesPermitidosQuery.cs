using Dtd.Application.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAlmacenesPermitidos;

public sealed record ListarAlmacenesPermitidosQuery(
    string Empresa)
    : IRequest<ErrorOr<IReadOnlyCollection<AlmacenDto>>>;