using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ObtenerConductor;

internal sealed class ObtenerConductorQueryHandler
    : IRequestHandler<
        ObtenerConductorQuery,
        ErrorOr<ConductorCatalogoDto>>
{
    private readonly IConductorRepository _conductorRepository;

    public ObtenerConductorQueryHandler(
        IConductorRepository conductorRepository)
    {
        _conductorRepository = conductorRepository;
    }

    public async Task<ErrorOr<ConductorCatalogoDto>> Handle(
        ObtenerConductorQuery request,
        CancellationToken cancellationToken)
    {
        var conductor = await _conductorRepository.GetByIdAsync(
            request.ConductorId,
            cancellationToken);

        if (conductor is null)
        {
            return Error.NotFound(
                "Conductor.NoEncontrado",
                $"No existe el conductor '{request.ConductorId}'.");
        }

        return new ConductorCatalogoDto
        {
            Id = conductor.Id,
            Nombre = conductor.Nombre,
            TaxId = conductor.TaxId,
            LicensePlate = conductor.LicensePlate,
            Channel = conductor.Canal.Valor,
            Email = conductor.Email?.Valor,
            Movil = conductor.Movil?.Valor,
            Language = conductor.Language
        };
    }
}