namespace Dtd.Application.AgenciaBases;

/// <summary>Read model de un agenciaBase del catálogo (agregado <c>AgenciaBase</c>) para la gestión por API
/// y para la selección del front (almacén/agencia → agencia-bases). A diferencia de
/// <see cref="Conductores.ConductorCatalogoDto"/>, incluye <c>Activo</c> (los catálogos gestionados lo
/// necesitan para la vista de gestión: activos e inactivos) y no tiene <c>LicensePlate</c> (es
/// destinatario, no driver).</summary>
public sealed record AgenciaBaseCatalogoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? TaxId { get; init; }
    public string? Direccion { get; init; }
    public string? CodigoPostal { get; init; }
    public string? Municipio { get; init; }
    public string? CodigoPaisIso { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Movil { get; init; }
    public string Language { get; init; } = "es";
    public bool Activo { get; init; }
}
