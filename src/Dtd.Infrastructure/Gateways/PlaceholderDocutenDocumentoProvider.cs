using Dtd.Application.GatewayContracts;
using Dtd.Application.Templates;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Empresas;
using Dtd.Domain.Templates;

namespace Dtd.Infrastructure.Gateways
{

    internal sealed class DocutenDocumentoProvider
        : IDocutenDocumentoProvider
    {
        private readonly IDocumentTemplateValuesBuilderResolver _builderResolver;

        public DocutenDocumentoProvider(
            IDocumentTemplateValuesBuilderResolver builderResolver)
        {
            _builderResolver = builderResolver;
        }

        public Task<DocutenDocumentoDto> ObtenerDocumentoAsync(
    DocumentoDigitalTransporte documento,
    Envio envio,
    EmpresaConfig empresa,
    Almacen almacen,
    Agencia agencia,
    Template template,
    IReadOnlyCollection<int> participantOrders,
    CancellationToken cancellationToken = default)
        {
            var builder = _builderResolver.Resolve(template.DocumentType);

            var values = builder.Build(
                documento,
                envio,
                empresa,
                almacen,
                agencia);

            var signers = BuildSigners(participantOrders);

            var dto = new DocutenDocumentoDto
            {
                DocumentType = template.DocumentType,
                DocumentName = BuildDocumentName(template, envio),
                ExternalId = $"EXT-{template.Code}-{envio.Referencia}",
                Signable = true,
                Template = new DocutenTemplateDto
                {
                    Code = template.Code,
                    Values = values
                },
                Signers = signers
            };

            return Task.FromResult(dto);
        }

        private static IReadOnlyList<DocutenSignerDto> BuildSigners(IReadOnlyCollection<int> participantOrders)
        {
            return participantOrders
                .OrderBy(x => x)
                .Select(order => new DocutenSignerDto
                {
                    Order = order
                })
                .ToArray();
        }

        private static string BuildDocumentName(
    Template template,
    Envio envio)
        {
            return $"{template.Code}-{envio.Referencia}.pdf";
        }
    }

}