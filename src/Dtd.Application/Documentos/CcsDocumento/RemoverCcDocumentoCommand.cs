using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.CcsDocumento;

/// <summary>
/// Quita un CC asignado a un documento (por su <c>Id</c> dentro del documento). Sólo mientras el
/// documento esté en estado <c>Nuevo</c>. Lanza (→ <see cref="ErrorType.Conflict"/>) si el documento ya
/// no es <c>Nuevo</c>, o <see cref="ErrorType.NotFound"/> si el CC no está asignado.
/// </summary>
public sealed record RemoverCcDocumentoCommand(Guid DocumentoId, Guid CcId)
    : IRequest<ErrorOr<Deleted>>;

internal sealed class RemoverCcDocumentoCommandHandler
    : IRequestHandler<RemoverCcDocumentoCommand, ErrorOr<Deleted>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public RemoverCcDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<Deleted>> Handle(RemoverCcDocumentoCommand request, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(request.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return Error.NotFound("Documento.NoEncontrado", $"No existe el documento '{request.DocumentoId}'.");
        }

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(documento.Empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{documento.Empresa}'.");
        }

        try
        {
            documento.RemoverCc(request.CcId);
        }
        catch (InvalidOperationException ex)
        {
            // RemoverCc comprueba primero el estado y luego la existencia: si el documento ya no está en
            // Nuevo fue el guard de estado (→ conflicto); si sí lo está, fue no-encontrado (→ 404).
            if (documento.Estado != EstadoDocumento.Nuevo)
            {
                return Error.Conflict("Documento.YaConfirmado", ex.Message);
            }

            return Error.NotFound(
                "Documento.CcNoAsignado",
                $"El CC '{request.CcId}' no está asignado al documento '{request.DocumentoId}'.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}
