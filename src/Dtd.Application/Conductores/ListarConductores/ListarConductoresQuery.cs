using Dtd.Domain.Agencias;
using Dtd.Domain.Conductores;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductores;

public sealed record ListarConductoresQuery(
    Guid AgenciaId)
    : IRequest<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>;

internal sealed class ListarConductoresQueryHandler
    : IRequestHandler<
        ListarConductoresQuery,
        ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;

    public ListarConductoresQueryHandler(
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository)
    {
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>> Handle(
        ListarConductoresQuery request,
        CancellationToken cancellationToken)
    {
        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{request.AgenciaId}' no existe en el catálogo.");
        }

        var conductores =
            await _conductorRepository.ListarPorAgenciaAsync(
                agencia.Id,
                cancellationToken);

        return conductores
            .Select(c => new ConductorCatalogoDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
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