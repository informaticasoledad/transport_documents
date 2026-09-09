using Dtd.Application.Conductores;

public sealed record ConductoresPaginadosDto(
    IReadOnlyList<ConductorCatalogoDto> Items,
    int Page,
    int PageSize,
    int Total);