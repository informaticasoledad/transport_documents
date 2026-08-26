namespace Dtd.Application.Documentos.Contracts
{
    public interface IDocumentReferenceGenerator
    {
        Task<string> GenerateAsync(
            string empresa,
            string almacen,
            DateTime fecha,
            CancellationToken cancellationToken = default);
    }
}