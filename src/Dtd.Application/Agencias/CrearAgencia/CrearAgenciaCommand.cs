using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.CrearAgencia;

public sealed record CrearAgenciaCommand(
    string Codigo,
    string Nombre,
    string? AgenciaQs,
    bool EntregaEnDestino)
    : IRequest<ErrorOr<AgenciaDto>>;