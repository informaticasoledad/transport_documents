using Dtd.Application.Conductores;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarConductoresDefecto;

/// <summary>Lista los conductores por defecto de una tupla (empresa, almacén, agencia) para que el
/// front los auto-adjunte al generar un documento para esa tupla. Pueden ser varios; sólo los activos.
/// El back no los auto-adjunta — los añade el front vía <c>POST /documentos/{id}/conductores</c>.</summary>
public sealed record ListarConductoresDefectoQuery(string Empresa, string AlmacenCodigo, string AgenciaCodigo)
    : IRequest<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>;

internal sealed class ListarConductoresDefectoQueryHandler
    : IRequestHandler<ListarConductoresDefectoQuery, ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarConductoresDefectoQueryHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>> Handle(
        ListarConductoresDefectoQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        // Defense-in-depth: el almacén debe existir y la agencia estar disponible para él (unión
        // almacen_agencias). Si la tupla no existe, no tiene sentido listar defaults. Este endpoint
        // de selección se queda por código (el catálogo de conductores-default se resuelve por códigos);
        // la validación de disponibilidad, al ir por IDs internamente, resuelve aquí el Id de la agencia.
        var almacen = await _almacenRepository.GetByEmpresaYCodigoAsync(empresa, request.AlmacenCodigo, cancellationToken);
        if (almacen is null)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenCodigo}' no existe para la empresa '{empresa}'.");
        }

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(empresa, request.AgenciaCodigo, cancellationToken);
        if (agencia is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible para el almacén '{request.AlmacenCodigo}' (empresa '{empresa}').");
        }

        var disponible = await _almacenRepository.EsAgenciaDisponibleAsync(
            almacen.Id, agencia.Id, cancellationToken);
        if (!disponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible para el almacén '{request.AlmacenCodigo}' (empresa '{empresa}').");
        }

        var conductores = await _conductorRepository.ObtenerConductoresDefectoAsync(
            empresa, request.AlmacenCodigo, request.AgenciaCodigo, cancellationToken);

        return conductores
            .Select(c => new ConductorCatalogoDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Movil = c.Movil?.Valor,
                Language = c.Language
            })
            .ToList();
    }
}