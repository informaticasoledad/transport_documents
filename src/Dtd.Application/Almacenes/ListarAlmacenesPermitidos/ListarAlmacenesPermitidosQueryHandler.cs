using Dtd.Application.Common.Security;
using Dtd.Domain.Almacenes;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAlmacenesPermitidos;

internal sealed class ListarAlmacenesPermitidosQueryHandler
    : IRequestHandler<
        ListarAlmacenesPermitidosQuery,
        ErrorOr<IReadOnlyCollection<AlmacenDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IUserPermissionService _userPermissionService;

    public ListarAlmacenesPermitidosQueryHandler(
        IAlmacenRepository almacenRepository,
        IUserPermissionService userPermissionService)
    {
        _almacenRepository = almacenRepository;
        _userPermissionService = userPermissionService;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AlmacenDto>>> Handle(
        ListarAlmacenesPermitidosQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var codigosPermitidos =
            await _userPermissionService.GetAllowedWarehousesAsync(
                empresa,
                cancellationToken);

        if (codigosPermitidos.Count == 0)
        {
            return Array.Empty<AlmacenDto>();
        }

        var almacenes =
            await _almacenRepository.ListarPorEmpresaAsync(
                empresa,
                cancellationToken);

        var codigosPermitidosSet = codigosPermitidos
            .ToHashSet(StringComparer.OrdinalIgnoreCase);


        var resultado = almacenes
            .Where(a => codigosPermitidosSet.Contains(a.Codigo))
            .OrderBy(a => a.Nombre)
            .Select(a => new AlmacenDto(
                a.Id,
                a.Codigo,
                a.Nombre,
                a.Direccion,
                a.CodigoPostal,
                a.Ciudad,
                a.CodigoPaisIso,
                a.Email.ToString(),
                a.Telefono))
            .ToList();

        return resultado;
    }
}