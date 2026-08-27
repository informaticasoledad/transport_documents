using Dtd.Application.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.SincronizarEstadoDocuten;

internal sealed class SincronizarEstadoDocutenCommandHandler
    : IRequestHandler<
        SincronizarEstadoDocutenCommand,
        ErrorOr<DocumentoEstadoDto>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly GatewayContracts.IDocutenGateway _docutenGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public SincronizarEstadoDocutenCommandHandler(
        IDocumentoRepository documentoRepository,
        GatewayContracts.IDocutenGateway docutenGateway,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _docutenGateway = docutenGateway;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<DocumentoEstadoDto>> Handle(
        SincronizarEstadoDocutenCommand request,
        CancellationToken cancellationToken)
    {
        var documento =
            await _documentoRepository.GetByIdAsync(
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

        /*
        TODO: revisar la sincronización manual con Docuten.

        if (string.IsNullOrWhiteSpace(documento.PlataformaId))
        {
            return Error.Validation(
                "Documento.NoEnviadoADocuten",
                "El documento aún no ha sido transmitido a Docuten.");
        }

        var estado = await _docutenGateway.ObtenerEstadoAsync(
            documento.PlataformaId,
            cancellationToken);

        documento.ActualizarEstadoDocuten(
            estado.Estado);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new DocumentoEstadoDto(
            documento.Id,
            documento.Estado.ToString(),
            documento.PlataformaEstado);
        */

        // TODO: eliminar cuando se implemente la sincronización real.
        return new DocumentoEstadoDto(
            documento.Id,
            "",
            "");
    }
}