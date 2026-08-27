using Dtd.Application.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.CcsDocumento;

/// <summary>
/// Quita un CC asignado a un documento (por su <c>Id</c> dentro del documento).
/// Sólo mientras el documento esté en estado <c>Nuevo</c>.
/// Lanza (→ <see cref="ErrorType.Conflict"/>) si el documento ya no es <c>Nuevo</c>,
/// o <see cref="ErrorType.NotFound"/> si el CC no está asignado.
/// </summary>
public sealed record RemoverCcDocumentoCommand(
    Guid DocumentoId,
    Guid CcId)
    : IRequest<ErrorOr<Deleted>>;

internal sealed class RemoverCcDocumentoCommandHandler
    : IRequestHandler<RemoverCcDocumentoCommand, ErrorOr<Deleted>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public RemoverCcDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        RemoverCcDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(
            request.DocumentoId,
            cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                documento.Empresa,
                documento.AlmacenId,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        try
        {
            documento.RemoverCc(
                request.CcId);
        }
        catch (InvalidOperationException ex)
        {
            // RemoverCc comprueba primero el estado y luego la existencia:
            // si el documento ya no está en Nuevo → conflicto;
            // si sigue en Nuevo pero no existe el CC → 404.
            if (documento.Estado != EstadoDocumento.Nuevo)
            {
                return Error.Conflict(
                    "Documento.YaConfirmado",
                    ex.Message);
            }

            return Error.NotFound(
                "Documento.CcNoAsignado",
                $"El CC '{request.CcId}' no está asignado " +
                $"al documento '{request.DocumentoId}'.");
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Deleted;
    }
}