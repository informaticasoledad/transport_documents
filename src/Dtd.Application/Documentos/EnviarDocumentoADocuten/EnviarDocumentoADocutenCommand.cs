using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EnviarDocumentoADocuten;

/// <summary>
/// Transmite a Docuten un documento previamente generado (estado Nuevo) y lo pasa a Enviando. Los
/// conductores se asignan al generar (auto-default de la tupla almacén/agencia) o a mano mientras el
/// documento esté en Nuevo; aquí sólo se valida que haya ≥1 conductor y que todos tengan canal de
/// comunicación adecuado (email/sms/whatsapp). Si la transmisión falla, registra el intento y
/// mantiene el documento en Nuevo para reintentar.
/// </summary>
public sealed record EnviarDocumentoADocutenCommand(Guid DocumentoId) : IRequest<ErrorOr<DocumentoEnviadoDto>>;

public sealed record DocumentoEnviadoDto(Guid DocumentoId, string LotId, string Estado);