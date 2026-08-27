using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAlmacenes;

/// <summary>
/// Lista los almacenes activos de una empresa a los que tiene acceso el usuario,
/// para el dropdown de selección del front.
/// </summary>
public sealed record ListarAlmacenesQuery(
    string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<AlmacenDto>>>;

internal sealed class ListarAlmacenesQueryHandler
    : IRequestHandler<
        ListarAlmacenesQuery,
        ErrorOr<IReadOnlyList<AlmacenDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAlmacenesQueryHandler(
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AlmacenDto>>> Handle(
        ListarAlmacenesQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacenesPermitidos =
            await _accesoAlmacenService.ObtenerAlmacenesPermitidosAsync(
                empresa,
                cancellationToken);

        if (almacenesPermitidos.IsError)
        {
            return almacenesPermitidos.Errors;
        }

        var idsPermitidos = almacenesPermitidos.Value;

        var almacenes =
            await _almacenRepository.ListarPorEmpresaAsync(
                empresa,
                cancellationToken);

        return almacenes
            .Where(a => idsPermitidos.Contains(a.Id))
            .Select(a => new AlmacenDto(
                a.Id,
                a.Codigo,
                a.Nombre,
                a.Direccion,
                a.CodigoPostal,
                a.Ciudad,
                a.CodigoPaisIso,
                a.Email?.Valor,
                a.Telefono))
            .ToList();
    }
}