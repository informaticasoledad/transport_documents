using Dtd.Domain.Agencias;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ListarAgencias;

/// <summary>
/// Lista las agencias activas del catálogo,
/// para el dropdown de selección del front.
/// </summary>
public sealed record ListarAgenciasQuery
    : IRequest<ErrorOr<IReadOnlyList<AgenciaDto>>>;

internal sealed class ListarAgenciasQueryHandler
    : IRequestHandler<
        ListarAgenciasQuery,
        ErrorOr<IReadOnlyList<AgenciaDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;

    public ListarAgenciasQueryHandler(
        IAgenciaRepository agenciaRepository)
    {
        _agenciaRepository = agenciaRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaDto>>> Handle(
        ListarAgenciasQuery request,
        CancellationToken cancellationToken)
    {
        var agencias =
            await _agenciaRepository.ListarActivasAsync(
                cancellationToken);

        return agencias
            .Select(a => new AgenciaDto(
                a.Id,
                a.Codigo,
                a.Nombre,
                a.Activa,
                a.AgenciaQs,
                a.EnvioDirecto))
            .ToList();
    }
}