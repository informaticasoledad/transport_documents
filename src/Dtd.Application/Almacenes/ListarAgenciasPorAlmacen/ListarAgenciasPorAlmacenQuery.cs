using Dtd.Application.Security;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAgenciasPorAlmacen;

/// <summary>Lista las agencias (carriers) disponibles para un almacén de una empresa
/// (unión <c>almacen_agencias</c>), para el dropdown de selección del front. Los conductores por
/// defecto de cada tupla se consultan por separado vía endpoint dedicado.</summary>
public sealed record ListarAgenciasPorAlmacenQuery(string Empresa, string AlmacenCodigo)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaDto>>>;

internal sealed class ListarAgenciasPorAlmacenQueryHandler
    : IRequestHandler<ListarAgenciasPorAlmacenQuery, ErrorOr<IReadOnlyList<AgenciaDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarAgenciasPorAlmacenQueryHandler(
        IAlmacenRepository almacenRepository,
        IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaDto>>> Handle(
        ListarAgenciasPorAlmacenQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agencias = await _almacenRepository.ListarAgenciasDisponiblesAsync(
            empresa, request.AlmacenCodigo, cancellationToken);

        return agencias
            .Select(a => new AgenciaDto(a.Id, a.Codigo, a.Nombre))
            .ToList();
    }
}