using Dtd.Application.Agencias;
using Dtd.Application.Almacenes;
using Dtd.Application.Documentos;
using Dtd.Application.Templates;
using Dtd.Domain.Almacenes;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAgenciasPorAlmacen;

/// <summary>
/// Lista las agencias (carriers) disponibles para un almacén de una empresa
/// (unión <c>almacen_agencias</c>), para el dropdown de selección del front.
/// Los conductores por defecto de cada tupla se consultan por separado
/// vía endpoint dedicado.
/// </summary>
public sealed record ListarAgenciasPorAlmacenQuery(
    string Empresa,
    Guid AlmacenId)
    : IRequest<ErrorOr<IReadOnlyList<AlmacenAgenciaDetalleDto>>>;

internal sealed class ListarAgenciasPorAlmacenQueryHandler
    : IRequestHandler<
        ListarAgenciasPorAlmacenQuery,
        ErrorOr<IReadOnlyList<AlmacenAgenciaDetalleDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarAgenciasPorAlmacenQueryHandler(
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AlmacenAgenciaDetalleDto>>> Handle(
        ListarAgenciasPorAlmacenQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null || almacen.Empresa != empresa)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                empresa,
                almacen.Id,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        var relaciones =
            await _almacenRepository.ListarRelacionesAgenciasAsync(
                almacen.Id,
                cancellationToken);

        return relaciones
       .Select(r => new AlmacenAgenciaDetalleDto(
           r.Agencia.Id,
           r.Agencia.Codigo,
           r.Agencia.Nombre,
           r.Agencia.EntregaEnDestino,
           r.Agencia.RequierePrecinto,

           new TemplateDto(
               r.Template.Id,
               r.Template.Empresa,
               r.Template.Code,
               r.Template.DocumentType,
               r.Template.Name,
               r.Template.Language,
               r.Template.Active),

           r.AgenciaBase is null
               ? null
               : new AgenciaBaseDto(
                   r.AgenciaBase.Id,
                   r.AgenciaBase.Codigo,
                   r.AgenciaBase.Nombre,
                   r.AgenciaBase.Direccion,
                   r.AgenciaBase.CodigoPostal,
                   r.AgenciaBase.Municipio,
                   r.AgenciaBase.CodigoPaisIso,
                   r.AgenciaBase.Email?.Valor,
                   r.AgenciaBase.Language,
                   r.AgenciaBase.Activo),

           r.ConductoresDefecto
               .Select(x => new ConductorDto
               {
                   Id = x.Conductor.Id,
                   Nombre = x.Conductor.Nombre,
                   TaxId = x.Conductor.TaxId,
                   LicensePlate = x.Conductor.LicensePlate,
                   Channel = x.Conductor.Canal.Valor,
                   Email = x.Conductor.Email?.Valor,
                   Movil = x.Conductor.Movil?.Valor,
                   Language = x.Conductor.Language
               })
               .ToList(),

           r.Ccs
               .Where(x => x.PorDefecto)
               .Select(x => new CcDto
               {
                   Id = x.Cc.Id,
                   Codigo = x.Cc.Codigo,
                   Nombre = x.Cc.Nombre,
                   Email = x.Cc.Email?.Valor,
                   Language = x.Cc.Language
               })
               .ToList()
       ))
       .ToList();
    }
}