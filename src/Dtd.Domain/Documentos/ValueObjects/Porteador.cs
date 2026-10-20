namespace Dtd.Domain.Documentos.ValueObjects;

public sealed record Porteador
{
    public string Nombre { get; }
    public string IdentificadorFiscal { get; }

    public string? Direccion { get; }
    public string? CodigoPostal { get; }
    public string? Municipio { get; }
    public string? CodigoPaisIso { get; }

    private Porteador(
        string nombre,
        string identificadorFiscal,
        string? direccion,
        string? codigoPostal,
        string? municipio,
        string? codigoPaisIso)
    {
        Nombre = nombre;
        IdentificadorFiscal = identificadorFiscal;
        Direccion = direccion;
        CodigoPostal = codigoPostal;
        Municipio = municipio;
        CodigoPaisIso = codigoPaisIso;
    }

    public static Porteador Crear(
        string nombre,
        string identificadorFiscal,
        string? direccion = null,
        string? codigoPostal = null,
        string? municipio = null,
        string? codigoPaisIso = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre del porteador es obligatorio.",
                nameof(nombre));

        if (string.IsNullOrWhiteSpace(identificadorFiscal))
            throw new ArgumentException(
                "El identificador fiscal del porteador es obligatorio.",
                nameof(identificadorFiscal));

        return new Porteador(
            nombre.Trim(),
            identificadorFiscal.Trim(),
            Normalizar(direccion),
            Normalizar(codigoPostal),
            Normalizar(municipio),
            Normalizar(codigoPaisIso));
    }

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}