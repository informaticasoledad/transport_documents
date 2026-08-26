using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ListarAgencias;

/// <summary>Lista las agencias (carriers) activas de una empresa (catálogo <c>agencias</c>,
/// per-empresa), para el dropdown de selección del front.</summary>
public sealed record ListarAgenciasQuery(string Empresa) : IRequest<ErrorOr<IReadOnlyList<AgenciaDto>>>;

internal sealed class ListarAgenciasQueryHandler : IRequestHandler<ListarAgenciasQuery, ErrorOr<IReadOnlyList<AgenciaDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarAgenciasQueryHandler(IAgenciaRepository agenciaRepository, IUsuarioContexto usuarioContexto)
    {
        _agenciaRepository = agenciaRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaDto>>> Handle(ListarAgenciasQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agencias = await _agenciaRepository.ListarPorEmpresaAsync(empresa, cancellationToken);
        return agencias
            .Select(a => new AgenciaDto(a.Id, a.Codigo, a.Nombre))
            .ToList();
    }
}