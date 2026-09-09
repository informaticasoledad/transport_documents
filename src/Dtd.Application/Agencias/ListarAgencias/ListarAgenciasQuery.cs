using Dtd.Domain.Agencias;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ListarAgencias;

/// <summary>
/// Lista agencias con filtros y paginación.
/// </summary>
public sealed record ListarAgenciasQuery(
    string? Texto,
    bool? Activa,
    bool? EnvioDirecto,
    int Page = 1,
    int PageSize = 20)
    : IRequest<ErrorOr<AgenciasPaginadasDto>>;

public sealed record AgenciasPaginadasDto(
    IReadOnlyList<AgenciaDto> Items,
    int Page,
    int PageSize,
    int Total);

internal sealed class ListarAgenciasQueryHandler
    : IRequestHandler<
        ListarAgenciasQuery,
        ErrorOr<AgenciasPaginadasDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;

    public ListarAgenciasQueryHandler(
        IAgenciaRepository agenciaRepository)
    {
        _agenciaRepository = agenciaRepository;
    }

    public async Task<ErrorOr<AgenciasPaginadasDto>> Handle(
        ListarAgenciasQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var skip = (page - 1) * pageSize;

        var (agencias, total) =
            await _agenciaRepository.BuscarAsync(
                request.Texto,
                request.Activa,
                request.EnvioDirecto,
                skip,
                pageSize,
                cancellationToken);

        var items = agencias
            .Select(a => new AgenciaDto(
                a.Id,
                a.Codigo,
                a.Nombre,
                a.Activa,
                a.AgenciaQs,
                a.EnvioDirecto))
            .ToList();

        return new AgenciasPaginadasDto(
            items,
            page,
            pageSize,
            total);
    }
}