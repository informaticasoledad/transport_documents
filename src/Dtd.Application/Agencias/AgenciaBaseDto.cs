namespace Dtd.Application.Agencias;

public sealed record AgenciaBaseDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string? TaxId,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso,
    string? Movil,
    string? Email,
    string Canal,
    string Language,
    bool Activo);