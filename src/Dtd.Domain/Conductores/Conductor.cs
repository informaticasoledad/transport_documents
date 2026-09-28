using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Conductores;

/// <summary>
/// Agregado de referencia para un conductor (driver del lote Docuten).
/// Puede estar vinculado a una o varias agencias mediante la tabla
/// de persistencia <c>conductor_agencias</c>.
///
/// Guarda el perfil completo utilizado por Docuten:
/// nombre, identificación fiscal, matrícula, móvil, email,
/// canal de contacto e idioma.
///
/// Invariante:
/// - canal email -> requiere Email
/// - canal sms/whatsapp -> requiere Movil
///
/// La asignación a un documento realiza un snapshot de estos datos mediante
/// <see cref="Documentos.ConductorAsignado.CrearDesdeCatalogo"/>.
/// </summary>
public sealed class Conductor : AggregateRoot<Guid>
{
    public string Nombre { get; private set; }
    public string? TaxId { get; private set; }
    public string? LicensePlate { get; private set; }
    public string? TrailerLicensePlate { get; private set; }
    public Movil? Movil { get; private set; }
    public Email? Email { get; private set; }
    public Canal Canal { get; private set; }
    public string Language { get; private set; }
    public bool Activo { get; private set; }

    /// <summary>
    /// Usado por el ORM para materializar el agregado.
    /// </summary>
    private Conductor()
    {
        Nombre = string.Empty;
        Canal = null!;
        Language = "es";
    }

    private Conductor(
    string nombre,
    string? taxId,
    string? licensePlate,
    string? trailerLicensePlate,
    Movil? movil,
    Email? email,
    Canal canal,
    string language,
    bool activo)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        TaxId = taxId;
        LicensePlate = NormalizarOpcional(licensePlate);
        TrailerLicensePlate = NormalizarOpcional(trailerLicensePlate);
        Movil = movil;
        Email = email;
        Canal = canal;
        Language = language;
        Activo = activo;
    }

    public static Conductor Crear(
    string nombre,
    Canal canal,
    Movil? movil,
    Email? email,
    string? taxId = null,
    string? licensePlate = null,
    string? trailerLicensePlate = null,
    string language = "es")
    {
        ValidarDatos(
            nombre,
            canal,
            movil,
            email);

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        return new Conductor(
            nombre.Trim(),
            taxId?.Trim(),
            licensePlate,
            trailerLicensePlate,
            movil,
            email,
            canal,
            language.Trim(),
            activo: true);
    }

    public void Modificar(
    string nombre,
    Canal canal,
    Movil? movil,
    Email? email,
    string? taxId,
    string? licensePlate,
    string? trailerLicensePlate,
    string language)
    {
        ValidarDatos(
            nombre,
            canal,
            movil,
            email);

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        Nombre = nombre.Trim();
        TaxId = taxId?.Trim();
        LicensePlate = NormalizarOpcional(licensePlate);
        TrailerLicensePlate = NormalizarOpcional(trailerLicensePlate);
        Movil = movil;
        Email = email;
        Canal = canal;
        Language = language.Trim();
    }


    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    private static void ValidarDatos(
        string nombre,
        Canal canal,
        Movil? movil,
        Email? email)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de conductor es obligatorio.",
                nameof(nombre));
        }

        ArgumentNullException.ThrowIfNull(canal);

        if (canal.RequiereEmail && email is null)
        {
            throw new ArgumentException(
                $"El canal '{canal.Valor}' requiere un email de contacto.",
                nameof(email));
        }

        if (canal.RequiereMovil && movil is null)
        {
            throw new ArgumentException(
                $"El canal '{canal.Valor}' requiere un móvil de contacto.",
                nameof(movil));
        }
    }

    private static string? NormalizarOpcional(string? valor) =>
    string.IsNullOrWhiteSpace(valor)
        ? null
        : valor.Trim();
}