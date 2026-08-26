using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Templates;

namespace Dtd.Application.GatewayContracts;

public interface IDocutenDocumentoProvider
{
    Task<DocutenDocumentoDto> ObtenerDocumentoAsync(
        DocumentoDigitalTransporte documento,
        Envio envio,
        EmpresaConfig empresa,
        Almacen almacen,
        Agencia agencia,
        Template template,
        IReadOnlyCollection<int> participantOrders,
        CancellationToken cancellationToken = default);
}