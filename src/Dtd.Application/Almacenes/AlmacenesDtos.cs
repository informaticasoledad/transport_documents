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

/// <summary>Read model de una agencia disponible para un almacén (dropdown almacén → agencia). Expone
/// el <c>Id</c> (Guid) para que el front lo envíe en <c>generar</c>. Los conductores por defecto de la
/// tupla (almacén, agencia) se obtienen vía endpoint dedicado
/// <c>.../agencias/{agenciaCodigo}/conductores-default</c>, no van aquí.</summary>
public sealed record AgenciaDto(Guid Id, string Codigo, string Nombre);