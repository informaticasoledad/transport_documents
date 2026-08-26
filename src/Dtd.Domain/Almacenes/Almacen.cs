using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Almacenes;

/// <summary>
/// Agregado de referencia para un almacén (origen de expediciones). Es **local**: se mantiene en la
/// tabla <c>almacenes</c> (seed manual / CRUD futuro), no se recupera del ERP. Está scopeado por
/// empresa (la misma combinación empresa+almacén identifica orígenes comunes a varias agencias) y
/// lleva la dirección + contacto que el consignor del lote de Docuten necesita (el consignor combina
/// empresa + almacén). La relación M:N con <see cref="Agencias.Agencia"/> (qué carriers sirven a este
/// almacén) vive en la tabla de unión <c>almacen_agencias</c> (ver <c>IAlmacenRepository</c>).
/// </summary>
public sealed class Almacen : AggregateRoot<Guid>
{
    public string Empresa { get; private set; } = null!;
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string Direccion { get; private set; } = null!;
    public string CodigoPostal { get; private set; } = null!;
    public string Ciudad { get; private set; } = null!;
    public string CodigoPaisIso { get; private set; } = null!;
    public Email? Email { get; private set; } = null;
    public string? Telefono { get; private set; } = null;
    public string TipoFirmaConsignor { get; private set; } = "biometric";
    public bool Activo { get; private set; } = true;

    private Almacen()
    {
    }

    private Almacen(
        string empresa, string codigo, string nombre,
        string direccion, string codigoPostal, string ciudad, string codigoPaisIso,
        Email? email, string? telefono, string tipoFirmaConsignor, bool activo)
    {
        Id = Guid.NewGuid();
        Empresa = empresa;
        Codigo = codigo;
        Nombre = nombre;
        Direccion = direccion;
        CodigoPostal = codigoPostal;
        Ciudad = ciudad;
        CodigoPaisIso = codigoPaisIso;
        Email = email;
        Telefono = telefono;
        TipoFirmaConsignor = tipoFirmaConsignor;
        Activo = activo;
    }

    public static Almacen Crear(
    string empresa,
    string codigo,
    string nombre,
    string direccion,
    string codigoPostal,
    string ciudad,
    string codigoPaisIso,
    string? email = null,
    string? telefono = null)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException(
                "La empresa es obligatoria.",
                nameof(empresa));
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de almacén es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de almacén es obligatorio.",
                nameof(nombre));
        }

        return new Almacen(
            empresa.Trim(),
            codigo.Trim(),
            nombre.Trim(),
            direccion.Trim(),
            codigoPostal.Trim(),
            ciudad.Trim(),
            codigoPaisIso.Trim(),
            Email.Create(email),
            telefono?.Trim(),
            "biometric",
            activo: true);
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    /// <summary>Reemplaza la dirección del almacén (camalles opcionales, se triman).</summary>
    public void ActualizarDireccion(string direccion, string codigoPostal, string ciudad, string codigoPaisIso)
    {
        Direccion = direccion.Trim();
        CodigoPostal = codigoPostal.Trim();
        Ciudad = ciudad.Trim();
        CodigoPaisIso = codigoPaisIso.Trim();
    }

    /// <summary>Reemplaza el contacto del almacén. El email se normaliza vía <see cref="Email.Create"/>
    /// (lanza si tiene texto pero es inválido); el teléfono es un string libre (puede ser fijo).</summary>
    public void ActualizarContacto(string? email, string? telefono)
    {
        Email = Email.Create(email);
        Telefono = telefono?.Trim();
    }

    public void ConfigurarTipoFirmaConsignor(string tipoFirma)
    {
        if (string.IsNullOrWhiteSpace(tipoFirma))
        {
            throw new ArgumentException("El tipo de firma es obligatorio.", nameof(tipoFirma));
        }

        var valor = tipoFirma.Trim().ToLowerInvariant();
        if (valor is not ("biometric" or "automated"))
        {
            throw new ArgumentException("El tipo de firma debe ser biometric o automated.", nameof(tipoFirma));
        }

        TipoFirmaConsignor = valor;
    }
}
