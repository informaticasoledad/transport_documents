using Dtd.Application.Security;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs.ListarCcsPorAlmacen;

/// <summary>Lista los CCs activos del catálogo vinculados a un almacén de una empresa, para el dropdown
/// de selección del front. El almacén se resuelve por (empresa, almacenCodigo). Espejo de
/// <c>ListarAgenciaBasesPorAlmacenQuery</c>.</summary>
public sealed record ListarCcsPorAlmacenQuery(string Empresa, string AlmacenCodigo)
    : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class ListarCcsPorAlmacenQueryHandler
    : IRequestHandler<ListarCcsPorAlmacenQuery, ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarCcsPorAlmacenQueryHandler(
        IAlmacenRepository almacenRepository,
        ICcRepository ccRepository,
        IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _ccRepository = ccRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        ListarCcsPorAlmacenQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var almacen = await _almacenRepository.GetByEmpresaYCodigoAsync(empresa, request.AlmacenCodigo, cancellationToken);
        if (almacen is null)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenCodigo}' no existe para la empresa '{empresa}'.");
        }

        var ccs = await _ccRepository.ListarPorAlmacenAsync(almacen.Id, cancellationToken);
        return ccs.Select(CrearCcCommandHandler.ToDto).ToList();
    }
}