using Dtd.Application.Security;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases.ListarTodasAgenciaBases;

/// <summary>Lista TODOS los agencia-bases de una empresa (vista de gestión: activos e inactivos). A
/// diferencia de <c>ListarAgenciaBasesQuery</c>/<c>ListarAgenciaBasesPorAlmacenQuery</c> (que sólo listan
/// activos para la selección del front), ésta devuelve el catálogo completo para el CRUD de gestión.</summary>
public sealed record ListarTodasAgenciaBasesQuery(string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class ListarTodasAgenciaBasesQueryHandler
    : IRequestHandler<ListarTodasAgenciaBasesQuery, ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarTodasAgenciaBasesQueryHandler(
        IAgenciaBaseRepository agenciaBaseRepository,
        IUsuarioContexto usuarioContexto)
    {
        _agenciaBaseRepository = agenciaBaseRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        ListarTodasAgenciaBasesQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agenciaBases = await _agenciaBaseRepository.ListarPorEmpresaAsync(empresa, cancellationToken);
        return agenciaBases.Select(CrearAgenciaBaseCommandHandler.ToDto).ToList();
    }
}