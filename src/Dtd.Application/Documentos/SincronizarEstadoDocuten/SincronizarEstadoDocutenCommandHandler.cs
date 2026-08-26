using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.SincronizarEstadoDocuten;

internal sealed class SincronizarEstadoDocutenCommandHandler : IRequestHandler<SincronizarEstadoDocutenCommand, ErrorOr<DocumentoEstadoDto>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly GatewayContracts.IDocutenGateway _docutenGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public SincronizarEstadoDocutenCommandHandler(
        IDocumentoRepository documentoRepository,
        GatewayContracts.IDocutenGateway docutenGateway,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _docutenGateway = docutenGateway;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<DocumentoEstadoDto>> Handle(SincronizarEstadoDocutenCommand request, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(request.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return Error.NotFound("Documento.NoEncontrado", $"No existe el documento '{request.DocumentoId}'.");
        }

        // Autorización por empresa: el usuario debe tener acceso a la empresa del documento.
        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(documento.Empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{documento.Empresa}'.");
        }
        /* ojazo revisar
        if (string.IsNullOrWhiteSpace(documento.PlataformaId))
        {
            return Error.Validation(
                "Documento.NoEnviadoADocuten",
                "El documento aún no ha sido transmitido a Docuten.");
        }

        var estado = await _docutenGateway.ObtenerEstadoAsync(documento.PlataformaId, cancellationToken);

        documento.ActualizarEstadoDocuten(estado.Estado);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DocumentoEstadoDto(documento.Id, documento.Estado.ToString(), documento.PlataformaEstado);*/
        // ojazo revisar
        return new DocumentoEstadoDto(documento.Id, "", "");
    }
}
