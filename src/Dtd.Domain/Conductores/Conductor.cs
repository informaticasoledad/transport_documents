using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Conductores;

/// <summary>
/// Agregado de referencia para un conductor (driver del lote Docuten). Pertenece a una
/// <c>empresa</c> y se relaciona **M:N** con las agencias de esa empresa (vía la tabla de
/// persistencia <c>conductor_agencias</c>, no modelada en el agregado — un conductor puede
/// servir a varias agencias como DPDFR/DPDEU). Guarda el perfil completo del party Docuten:
/// <c>name</c>, <c>tax_id</c>, <c>license_plate</c>, <c>mobile</c>, <c>email</c>, <c>channel</c>
/// (<c>email</c>|<c>sms</c>|<c>whatsapp</c>) y <c>language</c>. Invariante: el contacto es
/// coherente con el canal (<c>email</c>→<c>Email</c>; <c>sms</c>/<c>whatsapp</c>→<c>Movil</c>).
/// El catálogo se mantiene en local (seed manual / CRUD futuro). La asignación a un documento
/// snapshot-ea los datos vía <see cref="Documentos.ConductorAsignado.CrearDesdeCatalogo"/>.
/// </summary>
public sealed class Conductor : AggregateRoot<Guid>
{
    public string Empresa { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public string? TaxId { get; private set; }
    public string? LicensePlate { get; private set; }
    public Movil? Movil { get; private set; }
    public Email? Email { get; private set; }
    public Canal Canal { get; private set; }
    public string Language { get; private set; }
    public bool Activo { get; private set; }

    /// <summary>Usado por el ORM para materializar el agregado; no para código de aplicación.</summary>
    private Conductor()
    {
        Empresa = string.Empty;
        Codigo = string.Empty;
        Nombre = string.Empty;
        Canal = null!;
        Language = "es";
    }

    private Conductor(
        string empresa, string codigo, string nombre, string? taxId, string? licensePlate,
        Movil? movil, Email? email, Canal canal, string language, bool activo)
    {
        Id = Guid.NewGuid();
        Empresa = empresa;
        Codigo = codigo;
        Nombre = nombre;
        TaxId = taxId;
        LicensePlate = licensePlate;
        Movil = movil;
        Email = email;
        Canal = canal;
        Language = language;
        Activo = activo;
    }

    /// <summary>
    /// Crea un conductor activo. Trima los textos y valida la coherencia canal-contacto
    /// (<paramref name="channel"/> = <c>email</c> → <paramref name="email"/> obligatorio;
    /// <c>sms</c>/<c>whatsapp</c> → <paramref name="movil"/> obligatorio). Lanza
    /// <see cref="ArgumentException"/> si faltan datos obligatorios o el contacto no corresponde al canal.
    /// </summary>
    public static Conductor Crear(
        string empresa, string codigo, string nombre, Canal channel,
        Movil? movil, Email? email,
        string? taxId = null, string? licensePlate = null, string language = "es")
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException("La empresa es obligatoria.", nameof(empresa));
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de conductor es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de conductor es obligatorio.", nameof(nombre));
        }

        ArgumentNullException.ThrowIfNull(channel);

        if (channel.RequiereEmail && email is null)
        {
            throw new ArgumentException(
                $"El canal '{channel.Valor}' requiere un email de contacto.", nameof(email));
        }

        if (channel.RequiereMovil && movil is null)
        {
            throw new ArgumentException(
                $"El canal '{channel.Valor}' requiere un móvil de contacto.", nameof(movil));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        return new Conductor(
            empresa.Trim(), codigo.Trim(), nombre.Trim(), taxId?.Trim(), licensePlate?.Trim(),
            movil, email, channel, language.Trim(), activo: true);
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
}