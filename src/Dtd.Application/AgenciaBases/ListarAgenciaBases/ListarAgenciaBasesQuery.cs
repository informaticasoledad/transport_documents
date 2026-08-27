using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.AgenciaBases;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases.ListarAgenciaBases;

/// <summary>
/// Lista los agencia-bases activos del catálogo vinculados a una agencia de una empresa,
/// para el dropdown de selección del front al asignar agenciaBase a un documento.
/// La agencia se resuelve por (empresa, agenciaCodigo).
/// Espejo de <c>ListarConductoresQuery</c>.
/// </summary>
public sealed record ListarAgenciaBasesQuery(
    string Empresa,
    string AgenciaCodigo)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class ListarAgenciaBasesQueryHandler
    : IRequestHandler<
        ListarAgenciaBasesQuery,
        ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAgenciaBasesQueryHandler(
        IAgenciaRepository agenciaRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _agenciaRepository = agenciaRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        ListarAgenciaBasesQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var accesoEmpresa =
            await _accesoAlmacenService.ValidarAccesoEmpresaAsync(
                empresa,
                cancellationToken);

        if (accesoEmpresa.IsError)
        {
            return accesoEmpresa.Errors;
        }

        var agencia =
            await _agenciaRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.AgenciaCodigo,
                cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{request.AgenciaCodigo}' de la empresa " +
                $"'{empresa}' no existe en el catálogo.");
        }

        var agenciaBases =
            await _agenciaBaseRepository.ListarActivosPorEmpresaAsync(
                empresa,
                cancellationToken);

        return agenciaBases
            .Select(CrearAgenciaBaseCommandHandler.ToDto)
            .ToList();
    }
}