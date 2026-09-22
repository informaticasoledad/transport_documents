using ErrorOr;
using MediatR;

namespace Dtd.Application.Empresas.ListarEmpresasPermitidas;

public sealed record ListarEmpresasPermitidasQuery
    : IRequest<ErrorOr<IReadOnlyCollection<EmpresaDto>>>;