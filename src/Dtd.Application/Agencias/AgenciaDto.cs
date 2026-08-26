namespace Dtd.Application.Agencias;

/// <summary>Read model de una agencia (carrier) de una empresa, para el dropdown de selección del front
/// (empresa → agencias). Expone el <c>Id</c> (Guid) para que el front lo pueda enviar en <c>generar</c>.
/// El catálogo <c>agencias</c> es per-empresa.</summary>
public sealed record AgenciaDto(Guid Id, string Codigo, string Nombre);