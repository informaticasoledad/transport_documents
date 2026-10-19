using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Documentos;

/// <summary>
/// Conductor asignado a un documento de transporte.
/// Conserva los datos del conductor en el momento de su asignación,
/// tanto si procede del catálogo como si se introduce manualmente.
/// </summary>
public sealed class ConductorAsignado : Entity<Guid>
{
    /// <summary>
    /// Identificador del catálogo, si el conductor procede de él.
    /// Se utiliza como clave de idempotencia para conductores de catálogo.
    /// </summary>
    public Guid? ConductorCatalogId { get; private set; }

    public string Nombre { get; private set; }
    public string? TaxId { get; private set; }

    public string? LicensePlate { get; private set; }
    public string? TrailerLicensePlate { get; private set; }

    public Movil? Movil { get; private set; }
    public Email? Email { get; private set; }
    public Canal Canal { get; private set; }
    public string Language { get; private set; }

    public bool EsManual => ConductorCatalogId is null;

    private ConductorAsignado()
    {
        Nombre = string.Empty;
        Canal = null!;
        Language = "es";
    }

    private ConductorAsignado(
        Guid? conductorCatalogId,
        string nombre,
        string? taxId,
        string? licensePlate,
        string? trailerLicensePlate,
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
        TrailerLicensePlate = trailerLicensePlate;
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
            conductor.TrailerLicensePlate,
            conductor.Movil,
            conductor.Email,
            conductor.Canal,
            conductor.Language);
    }

    /// <summary>
    /// Crea un conductor manual, sin incorporarlo al catálogo.
    /// La notificación se realiza siempre mediante SMS.
    /// Las matrículas se gestionan directamente en el documento.
    /// </summary>
    public static ConductorAsignado CrearManual(
        string nombre,
        string taxId,
        Movil movil)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre del conductor es obligatorio.",
                nameof(nombre));

        if (string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentException(
                "El identificador fiscal es obligatorio.",
                nameof(taxId));

        ArgumentNullException.ThrowIfNull(movil);

        return new ConductorAsignado(
            conductorCatalogId: null,
            nombre: nombre.Trim(),
            taxId: taxId.Trim(),
            licensePlate: null,
            trailerLicensePlate: null,
            movil: movil,
            email: null,
            canal: Canal.Create(Canal.Sms)!,
            language: "es");
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