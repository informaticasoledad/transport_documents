using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.CrearAgencia;

public sealed record CrearAgenciaCommand(
    string Codigo,
    string Nombre,
    string IdentificadorFiscal,
    string? AgenciaQs,
    bool EntregaEnDestino,
    bool RequierePrecinto,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso)
    : IRequest<ErrorOr<AgenciaDto>>;