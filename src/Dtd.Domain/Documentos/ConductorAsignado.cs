using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Documentos;

/// <summary>
/// Conductor asignado a un <see cref="DocumentoDigitalTransporte"/> (driver del lote Docuten).
/// Es una entidad hija del agregado y representa un snapshot inmutable del catálogo
/// <see cref="Conductor"/> en el momento de la asignación.
///
/// La idempotencia se controla mediante <see cref="ConductorCatalogId"/>.
/// </summary>
public sealed class ConductorAsignado : Entity<Guid>
{
    /// <summary>
    /// Id del conductor del catálogo en el momento de la asignación.
    /// Se utiliza como clave de idempotencia.
    /// </summary>
    public Guid ConductorCatalogId { get; private set; }

    public string Nombre { get; private set; }
    public string? TaxId { get; private set; }
    public string? LicensePlate { get; private set; }
    public Movil? Movil { get; private set; }
    public Email? Email { get; private set; }
    public Canal Canal { get; private set; }
    public string Language { get; private set; }

    /// <summary>
    /// Usado por el ORM para materializar la entidad.
    /// </summary>
    private ConductorAsignado()
    {
        Nombre = string.Empty;
        Canal = null!;
        Language = "es";
    }

    private ConductorAsignado(
        Guid conductorCatalogId,
        string nombre,
        string? taxId,
        string? licensePlate,
        Movil? movil,
        Email? email,
        Canal canal,
        string language)
    {
        Id = Guid.NewGuid();
        ConductorCatalogId = conductorCatalogId;
        Nombre = nombre;
        TaxId = taxId;
        LicensePlate = licensePlate;
        Movil = movil;
        Email = email;
        Canal = canal;
        Language = language;
    }

    /// <summary>
    /// Crea un snapshot del conductor del catálogo.
    /// </summary>
    public static ConductorAsignado CrearDesdeCatalogo(
        Conductor conductor)
    {
        ArgumentNullException.ThrowIfNull(conductor);

        return new ConductorAsignado(
            conductor.Id,
            conductor.Nombre,
            conductor.TaxId,
            conductor.LicensePlate,
            conductor.Movil,
            conductor.Email,
            conductor.Canal,
            conductor.Language);
    }

    /// <summary>
    /// Indica si los datos de contacto son coherentes con el canal.
    /// </summary>
    public bool TieneCanalValido =>
        Canal.RequiereEmail
            ? Email is not null
            : Canal.RequiereMovil
                ? Movil is not null
                : false;
}