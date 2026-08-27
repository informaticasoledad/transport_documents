using Dtd.Application.Almacenes;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases;

public sealed record EstablecerAgenciaBasesDefectoCommand(
    string Empresa,
    string AlmacenCodigo,
    string AgenciaCodigo,
    IReadOnlyList<Guid> AgenciaBaseIds)
    : IRequest<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>;

internal sealed class EstablecerAgenciaBasesDefectoCommandHandler
    : IRequestHandler<
        EstablecerAgenciaBasesDefectoCommand,
        ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EstablecerAgenciaBasesDefectoCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseCatalogoDto>>> Handle(
        EstablecerAgenciaBasesDefectoCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByEmpresaYCodigoAsync(
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

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(
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

        var idsUnicos = request.AgenciaBaseIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        foreach (var id in idsUnicos)
        {
            var agenciaBase =
                await _agenciaBaseRepository.GetByIdAsync(
                    id,
                    cancellationToken);

            if (agenciaBase is null ||
                agenciaBase.Empresa != empresa)
            {
                return Error.NotFound(
                    "AgenciaBase.NoEncontrado",
                    $"No existe el agenciaBase '{id}' " +
                    $"para la empresa '{empresa}'.");
            }

            if (!agenciaBase.Activo)
            {
                return Error.Validation(
                    "AgenciaBase.Inactivo",
                    $"El agenciaBase '{agenciaBase.Codigo}' no está activo.");
            }
        }

        await _agenciaBaseRepository.SetDefectosAsync(
            empresa,
            request.AlmacenCodigo,
            request.AgenciaCodigo,
            idsUnicos,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var defaults =
            await _agenciaBaseRepository.ObtenerAgenciaBasesDefectoAsync(
                empresa,
                request.AlmacenCodigo,
                request.AgenciaCodigo,
                cancellationToken);

        return defaults
            .Select(CrearAgenciaBaseCommandHandler.ToDto)
            .ToList();
    }
}