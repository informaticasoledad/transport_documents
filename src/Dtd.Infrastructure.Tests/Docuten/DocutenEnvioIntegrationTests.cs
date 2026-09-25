using Dtd.Application;
using Dtd.Application.Documentos.Mappers;
using Dtd.Application.Documentos.Pdf;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Mapping;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Infrastructure.Tests.Docuten;

public sealed class DocutenEnvioIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task EnviarDocumentoReal_TemplateMasPdf_A_Docuten()
    {
        // Arrange
        var documentoId = Guid.Parse(
            "493281de-0dd4-4b02-b23f-9bb49ca2cc07");

        using var provider =
            BuildServiceProvider();

        using var scope =
            provider.CreateScope();

        var documentoRepository =
            scope.ServiceProvider
                .GetRequiredService<IDocumentoRepository>();

        var empresaRepository =
            scope.ServiceProvider
                .GetRequiredService<IEmpresaRepository>();

        var almacenRepository =
            scope.ServiceProvider
                .GetRequiredService<IAlmacenRepository>();

        var agenciaRepository =
            scope.ServiceProvider
                .GetRequiredService<IAgenciaRepository>();

        var pdfGenerator =
            scope.ServiceProvider
                .GetRequiredService<IEnviosPdfGenerator>();

        var documentoProvider =
            scope.ServiceProvider
                .GetRequiredService<IDocutenDocumentoProvider>();

        var docutenGateway =
            scope.ServiceProvider
                .GetRequiredService<IDocutenGateway>();

        var mappingOptions =
            scope.ServiceProvider
                .GetRequiredService<DocutenMappingOptions>();

        // 1. Documento real
        var documento =
            await documentoRepository.GetByIdAsync(
                documentoId);

        Assert.NotNull(documento);

        // 2. Empresa
        var empresa =
            await empresaRepository.GetByEmpresaAsync(
                documento.Empresa);

        Assert.NotNull(empresa);

        // 3. Almacén
        var almacen =
            await almacenRepository.GetByIdAsync(
                documento.AlmacenId);

        Assert.NotNull(almacen);

        // 4. Agencia
        var agencia =
            await agenciaRepository.GetByIdAsync(
                documento.AgenciaId);

        Assert.NotNull(agencia);

        // 5. Relación almacén-agencia + template
        var almacenAgencia =
            await almacenRepository.GetRelacionAgenciaAsync(
                documento.AlmacenId,
                documento.AgenciaId);

        Assert.NotNull(almacenAgencia);
        Assert.NotNull(almacenAgencia.Template);

        // 6. Generamos PDF real
        var pdfDto =
            DocumentoEnviosPdfMapper.Map(documento);

        var pdfEnvios =
            pdfGenerator.Generate(pdfDto);

        Assert.NotEmpty(pdfEnvios);

        // 7. Construimos exactamente el lote que queremos probar
        var lote =
            await documento.ToDocutenLoteDto(
                empresa,
                almacen,
                agencia,
                almacenAgencia.Template,
                pdfEnvios,
                mappingOptions,
                documentoProvider,
                CancellationToken.None);

        // Comprobaciones antes de enviar.
        Assert.NotEmpty(lote.Shipments);

        foreach (var shipment in lote.Shipments)
        {
            Assert.NotNull(shipment.Documents);
            Assert.NotEmpty(shipment.Documents);

            var doc = shipment.Documents[0];

            // ESTA ES LA PRUEBA QUE NOS INTERESA:
            // ambos campos deben ir informados.
            Assert.NotNull(doc.Template);
            Assert.False(string.IsNullOrWhiteSpace(doc.Content));
        }

        // 8. Envío REAL a Docuten
        var resultado =
            await docutenGateway.EnviarAsync(
                lote,
                CancellationToken.None);

        // 9. Ver qué responde Docuten
        Assert.NotNull(resultado);
        Assert.False(
            string.IsNullOrWhiteSpace(resultado.LotId));

        Console.WriteLine(
            $"LotId: {resultado.LotId}");

        Console.WriteLine(
            $"Estado: {resultado.Estado}");

        foreach (var shipment in resultado.Shipments)
        {
            Console.WriteLine(
                $"Shipment: {shipment.ShipmentReference} " +
                $"- Id: {shipment.ShipmentId} " +
                $"- Estado: {shipment.ShipmentStatus}");
        }

        // IMPORTANTE:
        // NO llamamos a:
        //
        // documento.ConfirmarEnvioADocuten(...)
        // documento.ConfirmarEnvioPlataforma(...)
        // unitOfWork.SaveChangesAsync(...)
        //
        // Por tanto la BBDD local no se modifica.
    }

    private static IConfiguration BuildConfiguration()
    {
        var srcPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../"));

        var apiPath = Path.Combine(
            srcPath,
            "Dtd.Api");

        return new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile(
                "appsettings.json",
                optional: false)
            .AddJsonFile(
                "appsettings.Development.json",
                optional: true)
            .AddUserSecrets<DocutenEnvioIntegrationTests>()
            .AddEnvironmentVariables()
            .Build();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var configuration =
            BuildConfiguration();

        var services =
            new ServiceCollection();

        services.AddSingleton<IConfiguration>(
            configuration);

        services.AddApplication();
        services.AddInfrastructure(
            configuration);

        return services.BuildServiceProvider();
    }
}