using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EliminarAlmacenAgencia;

internal sealed class EliminarAlmacenAgenciaCommandHandler
    : IRequestHandler<
        EliminarAlmacenAgenciaCommand,
        ErrorOr<Success>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EliminarAlmacenAgenciaCommandHandler(
        IAlmacenRepository almacenRepository,
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<Success>> Handle(
        EliminarAlmacenAgenciaCommand request,
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

        var relacion =
            await _almacenRepository.GetRelacionAgenciaAsync(
                almacen.Id,
                request.AgenciaId,
                cancellationToken);

        if (relacion is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaId}' no está vinculada " +
                $"al almacén '{almacen.Codigo}'.");
        }

        var tieneDocumentos =
            await _documentoRepository.ExistenPorAlmacenYAgenciaAsync(
                almacen.Id,
                request.AgenciaId,
                cancellationToken);

        if (tieneDocumentos)
        {
            return Error.Conflict(
                "Almacen.AgenciaConDocumentos",
                "No se puede eliminar la relación entre el almacén " +
                "y la agencia porque existen documentos asociados.");
        }

        _almacenRepository.EliminarAgencia(relacion);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success;
    }
}