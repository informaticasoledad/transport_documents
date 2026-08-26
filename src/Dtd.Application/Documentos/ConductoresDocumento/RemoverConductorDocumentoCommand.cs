using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ConductoresDocumento;

/// <summary>
/// Quita un conductor asignado a un documento (por su <c>Id</c> dentro del documento). Sólo mientras
/// el documento esté en estado <c>Nuevo</c>. Lanza (→ <see cref="ErrorType.Conflict"/>) si el documento
/// ya no es <c>Nuevo</c>, o <see cref="ErrorType.NotFound"/> si el conductor no está asignado.
/// </summary>
public sealed record RemoverConductorDocumentoCommand(Guid DocumentoId, Guid ConductorId)
    : IRequest<ErrorOr<Deleted>>;

internal sealed class RemoverConductorDocumentoCommandHandler
    : IRequestHandler<RemoverConductorDocumentoCommand, ErrorOr<Deleted>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public RemoverConductorDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<Deleted>> Handle(RemoverConductorDocumentoCommand request, CancellationToken cancellationToken)
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
            documento.RemoverConductor(request.ConductorId);
        }
        catch (InvalidOperationException ex)
        {
            // RemoverConductor comprueba primero el estado y luego la existencia: si el documento ya no
            // está en Nuevo fue el guard de estado (→ conflicto); si sí lo está, fue no-encontrado (→ 404).
            if (documento.Estado != EstadoDocumento.Nuevo)
            {
                return Error.Conflict("Documento.YaConfirmado", ex.Message);
            }

            return Error.NotFound(
                "Documento.ConductorNoAsignado",
                $"El conductor '{request.ConductorId}' no está asignado al documento '{request.DocumentoId}'.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}