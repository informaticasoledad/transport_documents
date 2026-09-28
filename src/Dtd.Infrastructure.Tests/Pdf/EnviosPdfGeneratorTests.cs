using Dtd.Application;
using Dtd.Application.Documentos.Mappers;
using Dtd.Application.Documentos.Pdf;
using Dtd.Domain.Documentos;
using Dtd.Infrastructure.Pdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Infrastructure.Tests.Pdf;

public sealed class EnviosPdfGeneratorTests
{
    [Fact]
    public async Task Generate_DesdeDocumentoReal_GeneraUnPdfPorEnvio()
    {
        var documentoId =
            Guid.Parse("493281de-0dd4-4b02-b23f-9bb49ca2cc07");

        using var provider =
            BuildServiceProvider();

        using var scope =
            provider.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<IDocumentoRepository>();

        var generator =
            scope.ServiceProvider
                .GetRequiredService<IEnviosPdfGenerator>();

        var documento =
            await repository.GetByIdAsync(
                documentoId);

        Assert.NotNull(documento);
        Assert.NotEmpty(documento.Envios);

        var outputDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "GeneratedPdfs");

        Directory.CreateDirectory(outputDirectory);

        foreach (var envio in documento.Envios)
        {
            var dto =
                DocumentoEnviosPdfMapper.Map(
                    documento,
                    envio);

            var pdf =
                generator.Generate(dto);

            Assert.NotNull(pdf);
            Assert.NotEmpty(pdf);

            var outputPath = Path.Combine(
                outputDirectory,
                $"envio-{envio.Referencia}.pdf");

            await File.WriteAllBytesAsync(
                outputPath,
                pdf);

            Assert.True(File.Exists(outputPath));

            Console.WriteLine(
                $"PDF generado para envío {envio.Referencia}: {outputPath}");
        }
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
            .AddUserSecrets<EnviosPdfGeneratorTests>()
            .AddEnvironmentVariables()
            .Build();
    }
    private static ServiceProvider BuildServiceProvider()
    {
        var configuration = BuildConfiguration();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }
}