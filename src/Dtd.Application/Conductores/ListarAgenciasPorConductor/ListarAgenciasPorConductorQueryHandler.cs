using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;
using Dtd.Application.Agencias;

namespace Dtd.Application.Conductores.ListarAgenciasPorConductor;

internal sealed class ListarAgenciasPorConductorQueryHandler
    : IRequestHandler<
        ListarAgenciasPorConductorQuery,
        ErrorOr<IReadOnlyCollection<AgenciaDto>>>
{
    private readonly IConductorRepository _conductorRepository;

    public ListarAgenciasPorConductorQueryHandler(
        IConductorRepository conductorRepository)
    {
        _conductorRepository = conductorRepository;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AgenciaDto>>> Handle(
        ListarAgenciasPorConductorQuery request,
        CancellationToken cancellationToken)
    {
        var agencias = await _conductorRepository.ListarAgenciasAsync(
            request.ConductorId,
            cancellationToken);

        var result = agencias
            .Select(a => new AgenciaDto(
                a.Id,
                a.Codigo,
                a.Nombre,
                a.IdentificadorFiscal,
                a.Activa,
                a.AgenciaQs,
                a.EntregaEnDestino,
                a.RequierePrecinto, 
                a.Direccion, 
                a.CodigoPostal, 
                a.Municipio, 
                a.CodigoPaisIso )) 
            .ToList();

        return result;

    }
}