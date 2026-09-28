namespace Dtd.Application.Agencias;

public sealed record AgenciaBaseDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso,
    string? email,
    string Language,
    bool Activo);