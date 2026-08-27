using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>
/// Sustituye los CCs por defecto de una tupla (empresa, almacén, agencia) por los indicados
/// (replace de <c>almacen_agencia_ccs_defecto</c> para esa tupla).
/// Una lista vacía limpia los defaults.
/// <b>All-or-nothing:</b> antes de mutar valida que cada <c>ccId</c> exista y esté
/// vinculado a ambos, almacén y agencia.
/// </summary>
/// <returns>La lista de <see cref="CcCatalogoDto"/> de los defaults efectivos.</returns>
public sealed record EstablecerCcsDefectoCommand(
    string Empresa,
    string AlmacenCodigo,
    string AgenciaCodigo,
    IReadOnlyList<Guid> CcIds)
    : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class EstablecerCcsDefectoCommandHandler
    : IRequestHandler<
        EstablecerCcsDefectoCommand,
        ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EstablecerCcsDefectoCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        EstablecerCcsDefectoCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen =
            await _almacenRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.AlmacenCodigo,
                cancellationToken);

        if (almacen is null)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenCodigo}' no existe " +
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

        var agencia =
            await _agenciaRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.AgenciaCodigo,
                cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible " +
                $"para el almacén '{request.AlmacenCodigo}' " +
                $"(empresa '{empresa}').");
        }

        var disponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (!disponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible " +
                $"para el almacén '{request.AlmacenCodigo}' " +
                $"(empresa '{empresa}').");
        }

        // All-or-nothing: valida todos los CCs antes de mutar.
        var idsUnicos = request.CcIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        foreach (var id in idsUnicos)
        {
            var cc =
                await _ccRepository.GetByAlmacenYAgenciaEIdAsync(
                    almacen.Id,
                    agencia.Id,
                    id,
                    cancellationToken);

            if (cc is null)
            {
                return Error.NotFound(
                    "Cc.NoVinculado",
                    $"El CC '{id}' no está vinculado al almacén " +
                    $"'{request.AlmacenCodigo}' y la agencia " +
                    $"'{request.AgenciaCodigo}'.");
            }
        }

        await _ccRepository.SetDefectosAsync(
            empresa,
            request.AlmacenCodigo,
            request.AgenciaCodigo,
            idsUnicos,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var defaults =
            await _ccRepository.ObtenerCcsDefectoAsync(
                empresa,
                request.AlmacenCodigo,
                request.AgenciaCodigo,
                cancellationToken);

        return defaults
            .Select(CrearCcCommandHandler.ToDto)
            .ToList();
    }
}