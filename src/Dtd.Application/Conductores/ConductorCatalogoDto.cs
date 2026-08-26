namespace Dtd.Application.Conductores;

/// <summary>Read model de un conductor del catálogo (agregado <c>Conductor</c>) para la selección del
/// front (agencia → conductores). Sólo se listan los activos.</summary>
public sealed record ConductorCatalogoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? TaxId { get; init; }
    public string? LicensePlate { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Movil { get; init; }
    public string Language { get; init; } = "es";
}