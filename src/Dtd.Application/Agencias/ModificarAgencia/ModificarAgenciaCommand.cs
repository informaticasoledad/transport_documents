using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ModificarAgencia;

public sealed record ModificarAgenciaCommand(
    Guid AgenciaId,
    string Codigo,
    string Nombre,
    string IdentificadorFiscal,
    string? AgenciaQs,
    bool Activa,
    bool EntregaEnDestino)
    : IRequest<ErrorOr<AgenciaDto>>;