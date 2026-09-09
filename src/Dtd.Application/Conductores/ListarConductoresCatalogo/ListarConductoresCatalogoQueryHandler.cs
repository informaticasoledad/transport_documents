using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductoresCatalogo;

internal sealed class ListarConductoresCatalogoQueryHandler
    : IRequestHandler<
        ListarConductoresCatalogoQuery,
        ErrorOr<ConductoresPaginadosDto>>
{
    private readonly IConductorRepository _conductorRepository;

    public ListarConductoresCatalogoQueryHandler(
        IConductorRepository conductorRepository)
    {
        _conductorRepository = conductorRepository;
    }

    public async Task<ErrorOr<ConductoresPaginadosDto>> Handle(
        ListarConductoresCatalogoQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var skip = (page - 1) * pageSize;

        var (conductores, total) =
            await _conductorRepository.BuscarAsync(
                request.Texto,
                request.Activo,
                skip,
                pageSize,
                cancellationToken);

        var items = conductores
            .Select(c => new ConductorCatalogoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Movil = c.Movil?.Valor,
                Language = c.Language
            })
            .ToList();

        return new ConductoresPaginadosDto(
            items,
            page,
            pageSize,
            total);
    }
}