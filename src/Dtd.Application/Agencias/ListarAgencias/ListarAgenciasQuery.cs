using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ListarAgencias;

/// <summary>
/// Lista las agencias (carriers) activas de una empresa
/// (catálogo <c>agencias</c>, per-empresa),
/// para el dropdown de selección del front.
/// </summary>
public sealed record ListarAgenciasQuery(
    string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaDto>>>;

internal sealed class ListarAgenciasQueryHandler
    : IRequestHandler<
        ListarAgenciasQuery,
        ErrorOr<IReadOnlyList<AgenciaDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAgenciasQueryHandler(
        IAgenciaRepository agenciaRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _agenciaRepository = agenciaRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaDto>>> Handle(
        ListarAgenciasQuery request,
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

        var agencias =
            await _agenciaRepository.ListarPorEmpresaAsync(
                empresa,
                cancellationToken);

        return agencias
            .Select(a => new AgenciaDto(
                a.Id,
                a.Codigo,
                a.Nombre))
            .ToList();
    }
}