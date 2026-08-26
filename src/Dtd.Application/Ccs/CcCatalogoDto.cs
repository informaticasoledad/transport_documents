namespace Dtd.Application.Ccs;

/// <summary>Read model de un CC (destinatario en copia) del catálogo (agregado <c>Cc</c>) para la gestión
/// por API y para la selección del front (almacén/agencia → CCs). Es **email-only**: el único contacto es
/// <c>Email</c> (en Docuten siempre va como <c>signing_role="cc"</c> + <c>channel="email"</c>), sin
/// <c>Channel</c>/<c>Movil</c>/<c>TaxId</c>/<c>LicensePlate</c>. Incluye <c>Activo</c> para la vista de
/// gestión (activos e inactivos).</summary>
public sealed record CcCatalogoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Language { get; init; } = "es";
    public bool Activo { get; init; }
}