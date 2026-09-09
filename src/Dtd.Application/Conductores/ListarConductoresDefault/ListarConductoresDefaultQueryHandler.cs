using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductoresDefault;

internal sealed class ListarConductoresDefaultQueryHandler
    : IRequestHandler<
        ListarConductoresDefaultQuery,
        ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>
{
    private readonly IConductorRepository _conductorRepository;

    public ListarConductoresDefaultQueryHandler(
        IConductorRepository conductorRepository)
    {
        _conductorRepository = conductorRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>> Handle(
        ListarConductoresDefaultQuery request,
        CancellationToken cancellationToken)
    {
        var conductores =
            await _conductorRepository.ObtenerConductoresDefectoAsync(
                request.AlmacenId,
                request.AgenciaId,
                cancellationToken);

        return conductores
            .Select(c => new ConductorCatalogoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Movil = c.Movil?.Valor,
                Language = c.Language
            })
            .ToList();
    }
}