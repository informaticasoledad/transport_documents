using Dtd.Domain.Common;

namespace Dtd.Domain.Templates;

/// <summary>
/// Agregado de referencia para una plantilla de documento (p.ej. eCMR/consignment_note) de una
/// <c>empresa</c>. Identificada por un código estable **por empresa** (clave natural
/// <c>(empresa, code)</c>, igual que <c>Cc</c>, <c>AgenciaBase</c>, <c>Almacen</c> y <c>Agencia</c>).
/// Se gestiona por API (crear/actualizar/activar/desactivar). Una plantilla se asocia a un par
/// almacén-agencia concretos vía la columna <c>template_id</c> (FK nullable) de la join table
/// <c>almacen_agencias</c> (1:1, no catálogo M:N).
/// </summary>
public sealed class Template : AggregateRoot<Guid>
{
    public string Empresa { get; private set; }
    public string Code { get; private set; }
    public string DocumentType { get; private set; }
    public string Name { get; private set; }
    public string Language { get; private set; }
    public bool Active { get; private set; }

    /// <summary>Usado por el ORM para materializar el agregado; no para código de aplicación.</summary>
    private Template()
    {
        Empresa = string.Empty;
        Code = string.Empty;
        DocumentType = string.Empty;
        Name = string.Empty;
        Language = "es";
    }

    private Template(string empresa, string code, string documentType, string name, string language, bool active)
    {
        Id = Guid.NewGuid();
        Empresa = empresa;
        Code = code;
        DocumentType = documentType;
        Name = name;
        Language = language;
        Active = active;
    }

    /// <summary>
    /// Crea una plantilla activa. Trima los textos y normaliza <paramref name="language"/> a
    /// <c>"es"</c> si viene vacío/nulo. Lanza <see cref="ArgumentException"/> si faltan datos obligatorios.
    /// </summary>
    public static Template Crear(
        string empresa,
        string code,
        string documentType,
        string name,
        string? language = null)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException("La empresa es obligatoria.", nameof(empresa));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("El código de plantilla es obligatorio.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(documentType))
        {
            throw new ArgumentException("El tipo de documento es obligatorio.", nameof(documentType));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre de plantilla es obligatorio.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        return new Template(
            empresa.Trim(),
            code.Trim(),
            documentType.Trim(),
            name.Trim(),
            language.Trim(),
            active: true);
    }

    /// <summary>
    /// Actualiza los campos mutables de la plantilla (gestión por API). <c>Empresa</c> y <c>Code</c> son
    /// inmutables (identificadores) y no se tocan. No cambia <c>Active</c> (se gestiona con
    /// <see cref="Activar"/>/<see cref="Desactivar"/>).
    /// </summary>
    public void Actualizar(string documentType, string name, string language)
    {
        if (string.IsNullOrWhiteSpace(documentType))
        {
            throw new ArgumentException("El tipo de documento es obligatorio.", nameof(documentType));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre de plantilla es obligatorio.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        DocumentType = documentType.Trim();
        Name = name.Trim();
        Language = language.Trim();
    }

    public void Activar() => Active = true;
    public void Desactivar() => Active = false;
}