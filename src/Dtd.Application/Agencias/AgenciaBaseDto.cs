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
    //borrar string? Movil,
    string? email,
    //borrar string Canal,
    string Language,
    bool Activo);