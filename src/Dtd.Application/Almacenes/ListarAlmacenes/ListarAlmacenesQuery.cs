using Dtd.Application.Security;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAlmacenes;

/// <summary>Lista los almacenes activos de una empresa (para el dropdown de selección del front).</summary>
public sealed record ListarAlmacenesQuery(string Empresa) : IRequest<ErrorOr<IReadOnlyList<AlmacenDto>>>;

internal sealed class ListarAlmacenesQueryHandler : IRequestHandler<ListarAlmacenesQuery, ErrorOr<IReadOnlyList<AlmacenDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarAlmacenesQueryHandler(IAlmacenRepository almacenRepository, IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<AlmacenDto>>> Handle(ListarAlmacenesQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var almacenes = await _almacenRepository.ListarPorEmpresaAsync(empresa, cancellationToken);
        return almacenes
            .Select(a => new AlmacenDto(
                a.Id, a.Codigo, a.Nombre, a.Direccion, a.CodigoPostal, a.Ciudad, a.CodigoPaisIso, a.Email?.Valor, a.Telefono))
            .ToList();
    }
}