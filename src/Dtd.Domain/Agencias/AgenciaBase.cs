using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Agencias;

public sealed class AgenciaBase : Entity<Guid>
{
    public Guid AgenciaId { get; private set; }

    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public string? Direccion { get; private set; }
    public string? CodigoPostal { get; private set; }
    public string? Municipio { get; private set; }
    public string? CodigoPaisIso { get; private set; }

    public Email? Email { get; private set; }
    
    public string Language { get; private set; }

    public bool Activo { get; private set; }

    private AgenciaBase()
    {
        Codigo = string.Empty;
        Nombre = string.Empty;
        Language = "es";
    }

    internal AgenciaBase(
        Guid agenciaId,
        string codigo,
        string nombre,
        Email? email,
        string language = "es",
        string? direccion = null,
        string? codigoPostal = null,
        string? municipio = null,
        string? codigoPaisIso = null)
    {
        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La agencia es obligatoria.",
                nameof(agenciaId));
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de agencia base es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de agencia base es obligatorio.",
                nameof(nombre));
        }


        
        Id = Guid.NewGuid();
        AgenciaId = agenciaId;

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();

        Direccion = NormalizarOpcional(direccion);
        CodigoPostal = NormalizarOpcional(codigoPostal);
        Municipio = NormalizarOpcional(municipio);
        CodigoPaisIso = NormalizarOpcional(codigoPaisIso);
        Email = email;

        Language = NormalizarLanguage(language);

        Activo = true;
    }

    internal void Actualizar(
        string nombre,
        Email? email,
        string language,
        string? direccion = null,
        string? codigoPostal = null,
        string? municipio = null,
        string? codigoPaisIso = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de agencia base es obligatorio.",
                nameof(nombre));
        }

        
        Nombre = nombre.Trim();

        Direccion = NormalizarOpcional(direccion);
        CodigoPostal = NormalizarOpcional(codigoPostal);
        Municipio = NormalizarOpcional(municipio);
        CodigoPaisIso = NormalizarOpcional(codigoPaisIso);

        Email = email;

        Language = NormalizarLanguage(language);
    }

    internal void Activar() => Activo = true;

    internal void Desactivar() => Activo = false;

    public bool TieneDireccionCompleta =>
        !string.IsNullOrWhiteSpace(Direccion) &&
        !string.IsNullOrWhiteSpace(CodigoPostal) &&
        !string.IsNullOrWhiteSpace(Municipio) &&
        !string.IsNullOrWhiteSpace(CodigoPaisIso);


    private static string NormalizarLanguage(string? language) =>
        string.IsNullOrWhiteSpace(language)
            ? "es"
            : language.Trim();

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}