using Dtd.Application.Common.Security;
using Dtd.Application.Security;
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
    private readonly IContextoAccesoService _contextoAccesoService;

    public ListarAlmacenesPermitidosQueryHandler(
        IAlmacenRepository almacenRepository,
        IUserPermissionService userPermissionService,
        IContextoAccesoService contextoAccesoService)
    {
        _almacenRepository = almacenRepository;
        _userPermissionService = userPermissionService;
        _contextoAccesoService = contextoAccesoService;
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
            await _contextoAccesoService.GuardarAsync(
                new ContextoAcceso(
                    empresa,
                    Array.Empty<Guid>()),
                cancellationToken);

            return Array.Empty<AlmacenDto>();
        }

        var almacenes =
            await _almacenRepository.ListarPorEmpresaAsync(
                empresa,
                cancellationToken);

        var codigosPermitidosSet = codigosPermitidos
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var almacenesPermitidos = almacenes
            .Where(a => codigosPermitidosSet.Contains(a.Codigo))
            .OrderBy(a => a.Nombre)
            .ToList();

        var contexto = new ContextoAcceso(
            empresa,
            almacenesPermitidos
                .Select(a => a.Id)
                .ToList());

        await _contextoAccesoService.GuardarAsync(
            contexto,
            cancellationToken);

        var resultado = almacenesPermitidos
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