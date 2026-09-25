
namespace Dtd.Application.Documentos.Pdf;

public interface IEnviosPdfGenerator
{
    byte[] Generate(DocumentoEnviosPdfDto documento);
}