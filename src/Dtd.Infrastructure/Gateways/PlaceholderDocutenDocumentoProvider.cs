using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;

namespace Dtd.Infrastructure.Gateways;

internal sealed class PlaceholderDocutenDocumentoProvider
    : IDocutenDocumentoProvider
{
    private const string EcmrPlaceholderBase64 =
        "JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZyAvUGFnZXMgMiAwIFIgPj4KZW5kb2JqCjIgMCBvYmoKPDwgL1R5cGUgL1BhZ2VzIC9LaWRzIFszIDAgUl0gL0NvdW50IDEgPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhZ2UgL1BhcmVudCAyIDAgUiAvTWVkaWFCb3ggWzAgMCA1OTUgODQyXSAvUmVzb3VyY2VzIDw8IC9Gb250IDw8IC9GMSA1IDAgUiA+PiA+PiAvQ29udGVudHMgNCAwIFIgPj4KZW5kb2JqCjQgMCBvYmoKPDwgL0xlbmd0aCA0NCA+PgpzdHJlYW0KQlQgL0YxIDEyIFRmIDUwIDc5MCBUZCAoZUNNUiBEVEQgdGVzdCkgVGogRVQKZW5kc3RyZWFtCmVuZG9iago1IDAgb2JqCjw8IC9UeXBlIC9Gb250IC9TdWJ0eXBlIC9UeXBlMSAvQmFzZUZvbnQgL0hlbHZldGljYSA+PgplbmRvYmoKeHJlZiA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAwOSAwMDAwMCBuIAowMDAwMDAwMDU4IDAwMDAwIG4gCjAwMDAwMDAxMTUgMDAwMDAgbiAKMDAwMDAwMDI0MSAwMDAwMCBuIAowMDAwMDAwMzM1IDAwMDAwIG4gCnRyYWlsZXIKPDwgL1NpemUgNiAvUm9vdCAxIDAgUiA+PgpzdGFydHhyZWYKNDA1CiUlRU9G";

    public Task<DocutenDocumentoDto> ObtenerDocumentoAsync(
        DocumentoDigitalTransporte documento,
        Envio envio,
        CancellationToken cancellationToken = default)
    {
        var driversCount = documento.Conductores.Count;

        // De momento:
        // consignor + conductores + consignee
        var signersCount = 1 + driversCount + 1;

        var signers = new DocutenSignerDto[signersCount];

        for (var i = 0; i < signersCount; i++)
        {
            var order = i + 1;

            signers[i] = new DocutenSignerDto
            {
                Order = order,
                Coordinate = new DocutenSignerCoordinateDto
                {
                    SigPage = 0,
                    TopLeftCornerX = 120,
                    TopLeftCornerY = 650 - (order - 1) * 90,
                    Width = 180,
                    Height = 60
                }
            };
        }

        var dto = new DocutenDocumentoDto
        {
            DocumentType = "ecmr",
            DocumentName = $"eCMR-{envio.Referencia}.pdf",
            ExternalId = $"EXT-ECMR-{envio.Referencia}",
            Content = EcmrPlaceholderBase64,
            Signable = true,
            Signers = signers
        };

        return Task.FromResult(dto);
    }
}
