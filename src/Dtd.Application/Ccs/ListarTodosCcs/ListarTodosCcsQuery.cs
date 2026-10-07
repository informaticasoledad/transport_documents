using Dtd.Application.Almacenes;
using Dtd.Domain.Ccs;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs.ListarCcsCatalogo;

public sealed record ListarCcsCatalogoQuery(
    string Empresa,
    int Page = 1,
    int PageSize = 20,
    string? Texto = null,
    bool? Activo = null)
    : IRequest<ErrorOr<CcsPaginadosDto>>;

internal sealed class ListarCcsCatalogoQueryHandler
    : IRequestHandler<
        ListarCcsCatalogoQuery,
        ErrorOr<CcsPaginadosDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarCcsCatalogoQueryHandler(
        ICcRepository ccRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<CcsPaginadosDto>> Handle(
        ListarCcsCatalogoQuery request,
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

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var skip = (page - 1) * pageSize;

        var (ccs, total) =
            await _ccRepository.BuscarAsync(
                empresa,
                request.Texto,
                request.Activo,
                skip,
                pageSize,
                cancellationToken);

        var items = ccs
            .Select(CrearCcCommandHandler.ToDto)
            .ToList();

        return new CcsPaginadosDto(
            items,
            page,
            pageSize,
            total);
    }
}