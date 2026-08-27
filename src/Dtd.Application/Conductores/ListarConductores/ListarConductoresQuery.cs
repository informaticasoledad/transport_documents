using Dtd.Domain.Agencias;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductores;

/// <summary>
/// Lista los conductores activos del catálogo de una agencia de una empresa,
/// para el dropdown de selección del front al asignar conductores a un documento.
/// </summary>
public sealed record ListarConductoresQuery(
    string Empresa,
    string AgenciaCodigo)
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
        var empresa = request.Empresa.Trim();

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(
            empresa,
            request.AgenciaCodigo,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{request.AgenciaCodigo}' de la empresa '{empresa}' no existe en el catálogo.");
        }

        var conductores = await _conductorRepository.ListarPorAgenciaAsync(
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