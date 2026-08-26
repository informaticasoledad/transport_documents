using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Ccs;

/// <summary>
/// Agregado de referencia para un destinatario en copia (CC) de un lote Docuten. Pertenece a una
/// <c>empresa</c> y se relaciona **M:N** tanto con almacenes como con agencias de esa empresa (vía las
/// tablas de persistencia <c>cc_almacenes</c> y <c>cc_agencias</c>, no modeladas en el agregado): un CC
/// puede notificarse desde varias delegaciones (almacenes) y varios carriers (agencias). A diferencia
/// de <see cref="Conductores.Conductor"/> (seed-only), este catálogo se gestiona por API
/// (crear/actualizar/activar/desactivar), igual que <see cref="AgenciaBases.AgenciaBase"/>. Es **email-only**:
/// el único contacto es <see cref="Email"/> (obligatorio) —en Docuten el CC siempre va con
/// <c>signing_role="cc"</c> + <c>channel="email"</c>—, sin <c>Canal</c>/<c>Movil</c>/<c>TaxId</c>. La
/// asignación a un documento snapshot-ea los datos vía <see cref="Documentos.CcAsignado.CrearDesdeCatalogo"/>.
/// </summary>
public sealed class Cc : AggregateRoot<Guid>
{
    public string Empresa { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public Email Email { get; private set; }
    public string Language { get; private set; }
    public bool Activo { get; private set; }

    /// <summary>Usado por el ORM para materializar el agregado; no para código de aplicación.</summary>
    private Cc()
    {
        Empresa = string.Empty;
        Codigo = string.Empty;
        Nombre = string.Empty;
        Email = null!;
        Language = "es";
    }

    private Cc(string empresa, string codigo, string nombre, Email email, string language, bool activo)
    {
        Id = Guid.NewGuid();
        Empresa = empresa;
        Codigo = codigo;
        Nombre = nombre;
        Email = email;
        Language = language;
        Activo = activo;
    }

    /// <summary>
    /// Crea un CC activo. Trima los textos y exige <paramref name="email"/> no nulo (el CC es email-only:
    /// en Docuten siempre va como <c>signing_role="cc"</c> + <c>channel="email"</c>). Lanza
    /// <see cref="ArgumentException"/> si faltan datos obligatorios.
    /// </summary>
    public static Cc Crear(string empresa, string codigo, string nombre, Email email, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException("La empresa es obligatoria.", nameof(empresa));
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de CC es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de CC es obligatorio.", nameof(nombre));
        }

        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        return new Cc(empresa.Trim(), codigo.Trim(), nombre.Trim(), email, language.Trim(), activo: true);
    }

    /// <summary>
    /// Actualiza los campos mutables del CC (gestión por API). <c>Empresa</c> y <c>Codigo</c> son
    /// inmutables (identificadores) y no se tocan. Reexige <paramref name="email"/> no nulo. No cambia
    /// <c>Activo</c> (se gestiona con <see cref="Activar"/>/<see cref="Desactivar"/>).
    /// </summary>
    public void Actualizar(string nombre, Email email, string language)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de CC es obligatorio.", nameof(nombre));
        }

        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        Nombre = nombre.Trim();
        Email = email;
        Language = language.Trim();
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
}