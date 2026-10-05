namespace Dtd.Application.Documentos.DescargarPdfEnvio;

public sealed record DocumentoPdfResult(
    byte[] Content,
    string ContentType,
    string FileName);