namespace Dtd.Application.Almacenes;

/// <summary>Read model de un almacén para la selección del front (dropdown empresa → almacén).
/// Expone el <c>Id</c> (Guid) para que el front lo envíe en <c>generar</c> junto con el código.</summary>
public sealed record AlmacenDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string? Calle,
    string? CodigoPostal,
    string? Municipio,
    string? Pais,
    string? Email,
    string? Telefono);
public sealed record AgenciaDto(Guid Id, string Codigo, string Nombre);