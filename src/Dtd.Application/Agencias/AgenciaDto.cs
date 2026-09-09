namespace Dtd.Application.Agencias;

/// <summary>
/// Read model de una agencia de transporte.
/// Las agencias forman un catálogo global y se relacionan con los almacenes
/// mediante la configuración correspondiente.
/// </summary>
public sealed record AgenciaDto(
    Guid Id,
    string Codigo,
    string Nombre,
    bool Activa,
    string? AgenciaQs,
    bool EnvioDirecto);