using Dtd.Application.Ccs;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarCcsPorAlmacenAgencia
{

    public sealed record ListarCcsPorAlmacenAgenciaQuery(
        string Empresa,
        Guid AlmacenId,
        Guid AgenciaId)
        : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;
}