using Dtd.Application.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs.ListarTodosCcs;

/// <summary>
/// Lista TODOS los CCs de una empresa (vista de gestión: activos e inactivos).
/// Espejo de <c>ListarTodasAgenciaBasesQuery</c>.
/// </summary>
public sealed record ListarTodosCcsQuery(
    string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class ListarTodosCcsQueryHandler
    : IRequestHandler<
        ListarTodosCcsQuery,
        ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarTodosCcsQueryHandler(
        ICcRepository ccRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        ListarTodosCcsQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var accesoEmpresa =
            await _accesoAlmacenService.ValidarAccesoEmpresaAsync(
                empresa,
                cancellationToken);

        if (accesoEmpresa.IsError)
        {
            return accesoEmpresa.Errors;
        }

        var ccs =
            await _ccRepository.ListarPorEmpresaAsync(
                empresa,
                cancellationToken);

        return ccs
            .Select(CrearCcCommandHandler.ToDto)
            .ToList();
    }
}