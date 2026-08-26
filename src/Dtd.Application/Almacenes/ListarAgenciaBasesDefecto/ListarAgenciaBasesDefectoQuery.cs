using Dtd.Application.AgenciaBases;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarAgenciaBasesDefecto;

/// <summary>Lista los agencia-bases por defecto de una tupla (empresa, almacén, agencia) para que el
/// front los auto-adjunte al generar un documento para esa tupla. El back no los auto-adjunta — los
/// añade el front vía <c>POST /documentos/{id}/agencia-bases</c>. Espejo exacto de
/// <c>ListarConductoresDefectoQuery</c>.</summary>
public sealed record ListarAgenciaBasesDefectoQuery(string Empresa, string AlmacenCodigo, string AgenciaCodigo)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class ListarAgenciaBasesDefectoQueryHandler
    : IRequestHandler<ListarAgenciaBasesDefectoQuery, ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarAgenciaBasesDefectoQueryHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        ListarAgenciaBasesDefectoQuery request, CancellationToken cancellationToken)
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

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(empresa, request.AgenciaCodigo, cancellationToken);
        if (agencia is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible para el almacén '{request.AlmacenCodigo}' (empresa '{empresa}').");
        }

        var disponible = await _almacenRepository.EsAgenciaDisponibleAsync(almacen.Id, agencia.Id, cancellationToken);
        if (!disponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible para el almacén '{request.AlmacenCodigo}' (empresa '{empresa}').");
        }

        var agenciaBases = await _agenciaBaseRepository.ObtenerAgenciaBasesDefectoAsync(
            empresa, request.AlmacenCodigo, request.AgenciaCodigo, cancellationToken);

        return agenciaBases.Select(CrearAgenciaBaseCommandHandler.ToDto).ToList();
    }
}