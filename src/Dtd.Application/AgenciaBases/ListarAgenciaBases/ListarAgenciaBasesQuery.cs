using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases.ListarAgenciaBases;

/// <summary>Lista los agencia-bases activos del catálogo vinculados a una agencia de una empresa, para el
/// dropdown de selección del front al asignar agenciaBase a un documento. La agencia se resuelve por
/// (empresa, agenciaCodigo). Espejo de <c>ListarConductoresQuery</c>.</summary>
public sealed record ListarAgenciaBasesQuery(string Empresa, string AgenciaCodigo)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class ListarAgenciaBasesQueryHandler
    : IRequestHandler<ListarAgenciaBasesQuery, ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarAgenciaBasesQueryHandler(
        IAgenciaRepository agenciaRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IUsuarioContexto usuarioContexto)
    {
        _agenciaRepository = agenciaRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        ListarAgenciaBasesQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(empresa, request.AgenciaCodigo, cancellationToken);
        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{request.AgenciaCodigo}' de la empresa '{empresa}' no existe en el catálogo.");
        }

        var agenciaBases = await _agenciaBaseRepository.ListarActivosPorEmpresaAsync(empresa, cancellationToken);
        return agenciaBases.Select(CrearAgenciaBaseCommandHandler.ToDto).ToList();
    }
}
