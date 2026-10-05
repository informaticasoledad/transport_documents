using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.DescargarPdfEnvio;

internal sealed class DescargarPdfEnvioQueryHandler
    : IRequestHandler<DescargarPdfEnvioQuery, ErrorOr<DocumentoPdfResult>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocutenGateway _docutenGateway;

    public DescargarPdfEnvioQueryHandler(
        IDocumentoRepository documentoRepository,
        IDocutenGateway docutenGateway)
    {
        _documentoRepository = documentoRepository;
        _docutenGateway = docutenGateway;
    }

    public async Task<ErrorOr<DocumentoPdfResult>> Handle(
        DescargarPdfEnvioQuery request,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(
            request.DocumentoId,
            cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                code: "Documento.NotFound",
                description:
                    $"No se ha encontrado el documento '{request.DocumentoId}'.");
        }

        var envio = documento.Envios
            .FirstOrDefault(e => e.Id == request.EnvioId);

        if (envio is null)
        {
            return Error.NotFound(
                code: "Documento.Envio.NotFound",
                description:
                    $"No se ha encontrado el envío '{request.EnvioId}' en el documento.");
        }

        if (string.IsNullOrWhiteSpace(envio.PlataformaEnvioId))
        {
            return Error.Validation(
                code: "Documento.Envio.SinIdPlataforma",
                description:
                    "El envío todavía no tiene identificador de Docuten.");
        }

        var docutenDocument =
            await _docutenGateway.DescargarDocumentosEnvioAsync(
                envio.PlataformaEnvioId,
                cancellationToken);

        var fileName = string.IsNullOrWhiteSpace(docutenDocument.FileName)
            ? $"envio-{envio.Referencia}.pdf"
            : docutenDocument.FileName;

        return new DocumentoPdfResult(
            docutenDocument.Content,
            docutenDocument.ContentType,
            fileName);
    }
}