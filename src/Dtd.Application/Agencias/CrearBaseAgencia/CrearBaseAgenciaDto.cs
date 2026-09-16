namespace Dtd.Application.Agencias.CrearBaseAgencia;

public sealed record CrearBaseAgenciaDto(
    string Codigo,
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId = null,
    string Language = "es",
    string? Direccion = null,
    string? CodigoPostal = null,
    string? Municipio = null,
    string? CodigoPaisIso = null);