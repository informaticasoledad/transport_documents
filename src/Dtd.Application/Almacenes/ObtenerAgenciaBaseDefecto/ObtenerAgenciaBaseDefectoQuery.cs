using Dtd.Application.Agencias;
using ErrorOr;
using MediatR;

public sealed record ObtenerAgenciaBaseDefectoQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId)
    : IRequest<ErrorOr<AgenciaBaseDto?>>;