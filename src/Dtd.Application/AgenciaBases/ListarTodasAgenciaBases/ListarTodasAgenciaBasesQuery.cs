using Dtd.Application.Almacenes;
using Dtd.Domain.AgenciaBases;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases.ListarTodasAgenciaBases;

/// <summary>
/// Lista TODOS los agencia-bases de una empresa (vista de gestión: activos e inactivos).
/// A diferencia de <c>ListarAgenciaBasesQuery</c>/<c>ListarAgenciaBasesPorAlmacenQuery</c>
/// (que sólo listan activos para la selección del front), ésta devuelve el catálogo completo
/// para el CRUD de gestión.
/// </summary>
public sealed record ListarTodasAgenciaBasesQuery(
    string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class ListarTodasAgenciaBasesQueryHandler
    : IRequestHandler<
        ListarTodasAgenciaBasesQuery,
        ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarTodasAgenciaBasesQueryHandler(
        IAgenciaBaseRepository agenciaBaseRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _agenciaBaseRepository = agenciaBaseRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        ListarTodasAgenciaBasesQuery request,
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

        var agenciaBases =
            await _agenciaBaseRepository.ListarPorEmpresaAsync(
                empresa,
                cancellationToken);

        return agenciaBases
            .Select(CrearAgenciaBaseCommandHandler.ToDto)
            .ToList();
    }
}