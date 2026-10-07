namespace Dtd.Application.Ccs;

public sealed record CcsPaginadosDto(
    IReadOnlyList<CcCatalogoDto> Items,
    int Page,
    int PageSize,
    int Total);