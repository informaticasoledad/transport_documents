using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>
/// Sustituye los CCs por defecto de una tupla (empresa, almacén, agencia) por los indicados (replace de
/// <c>almacen_agencia_ccs_defecto</c> para esa tupla). Una lista vacía limpia los defaults.
/// <b>All-or-nothing:</b> antes de mutar valida que cada <c>ccId</c> exista y esté vinculado a AMBOS el
/// almacén y la agencia (defense-in-depth, vía <see cref="ICcRepository.GetByAlmacenYAgenciaEIdAsync"/>);
/// si alguno no, no se muta nada y se devuelve <c>Cc.NoVinculado</c>. Tras persistir, relee los defaults ya
/// filtrados (activos + vinculados a ambos) y los devuelve. Espejo de <c>EstablecerAgenciaBasesDefectoCommand</c>.
/// </summary>
/// <returns>La lista de <see cref="CcCatalogoDto"/> de los defaults efectivos.</returns>
public sealed record EstablecerCcsDefectoCommand(
    string Empresa,
    string AlmacenCodigo,
    string AgenciaCodigo,
    IReadOnlyList<Guid> CcIds) : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class EstablecerCcsDefectoCommandHandler
    : IRequestHandler<EstablecerCcsDefectoCommand, ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public EstablecerCcsDefectoCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        EstablecerCcsDefectoCommand request, CancellationToken cancellationToken)
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

        // All-or-nothing: valida TODOS los ccId (vinculados a AMBOS) antes de mutar.
        var idsUnicos = request.CcIds.Where(id => id != Guid.Empty).Distinct().ToList();
        foreach (var id in idsUnicos)
        {
            var cc = await _ccRepository.GetByAlmacenYAgenciaEIdAsync(
                almacen.Id, agencia.Id, id, cancellationToken);
            if (cc is null)
            {
                return Error.NotFound(
                    "Cc.NoVinculado",
                    $"El CC '{id}' no está vinculado a ambos el almacén '{request.AlmacenCodigo}' y la agencia '{request.AgenciaCodigo}'.");
            }
        }

        await _ccRepository.SetDefectosAsync(
            empresa, request.AlmacenCodigo, request.AgenciaCodigo, idsUnicos, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Releer los defaults efectivos (filtra activos + vinculados a ambos) y devolver.
        var defaults = await _ccRepository.ObtenerCcsDefectoAsync(
            empresa, request.AlmacenCodigo, request.AgenciaCodigo, cancellationToken);

        return defaults.Select(CrearCcCommandHandler.ToDto).ToList();
    }
}