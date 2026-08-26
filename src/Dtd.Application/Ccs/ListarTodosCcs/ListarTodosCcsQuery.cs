using Dtd.Application.Security;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs.ListarTodosCcs;

/// <summary>Lista TODOS los CCs de una empresa (vista de gestión: activos e inactivos). Espejo de
/// <c>ListarTodasAgenciaBasesQuery</c>.</summary>
public sealed record ListarTodosCcsQuery(string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class ListarTodosCcsQueryHandler
    : IRequestHandler<ListarTodosCcsQuery, ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarTodosCcsQueryHandler(
        ICcRepository ccRepository,
        IUsuarioContexto usuarioContexto)
    {
        _ccRepository = ccRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        ListarTodosCcsQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var ccs = await _ccRepository.ListarPorEmpresaAsync(empresa, cancellationToken);
        return ccs.Select(CrearCcCommandHandler.ToDto).ToList();
    }
}