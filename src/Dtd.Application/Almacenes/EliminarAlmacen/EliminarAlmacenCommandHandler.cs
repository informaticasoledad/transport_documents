using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EliminarAlmacen;

internal sealed class EliminarAlmacenCommandHandler
    : IRequestHandler<
        EliminarAlmacenCommand,
        ErrorOr<Deleted>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarAlmacenCommandHandler(
        IAlmacenRepository almacenRepository,
        IDocumentoRepository documentoRepository,
        IAccesoAlmacenService accesoAlmacenService,
        IUnitOfWork unitOfWork)
    {
        _almacenRepository = almacenRepository;
        _documentoRepository = documentoRepository;
        _accesoAlmacenService = accesoAlmacenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        EliminarAlmacenCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdParaActualizarAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null ||
            !string.Equals(
                almacen.Empresa,
                empresa,
                StringComparison.OrdinalIgnoreCase))
        {
            return Error.NotFound(
                "Almacen.NoEncontrado",
                $"No existe el almacén '{request.AlmacenId}' para la empresa '{empresa}'.");
        }

        var almacenesPermitidos =
            await _accesoAlmacenService.ObtenerAlmacenesPermitidosAsync(
                empresa,
                cancellationToken);

        if (almacenesPermitidos.IsError)
        {
            return almacenesPermitidos.Errors;
        }

        if (!almacenesPermitidos.Value.Contains(almacen.Id))
        {
            return Error.Forbidden(
                "Almacen.SinAcceso",
                "El usuario no tiene acceso al almacén indicado.");
        }

        var tieneDocumentos =
            await _documentoRepository.ExistenPorAlmacenAsync(
                almacen.Id,
                cancellationToken);

        if (tieneDocumentos)
        {
            return Error.Conflict(
                "Almacen.TieneDocumentos",
                "No se puede eliminar el almacén porque tiene documentos asociados.");
        }

        _almacenRepository.Remove(almacen);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Deleted;
    }
}