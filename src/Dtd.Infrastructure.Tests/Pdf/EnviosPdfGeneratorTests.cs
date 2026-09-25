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
    public void Generate_GeneraPdfConEnviosYExpediciones()
    {
        // Arrange
        var dto = new DocumentoEnviosPdfDto(
            DocumentoId: Guid.NewGuid(),
            Empresa: "001",
            Referencia: "DDT-TEST-001",
            FechaCreacion: new DateTime(2026, 9, 25, 12, 0, 0),
            Envios:
            [
                new DocumentoEnvioPdfDto(
                    Referencia: "ENVIO-001",
                    Destino: "CLIENTE PRUEBA A",
                    Direccion: "C/ Mayor, 15",
                    CodigoPostal: "03660",
                    Ciudad: "Novelda",
                    CodigoPais: "ES",
                    TotalBultos: 15,
                    TotalPeso: 164.50m,
                    Expediciones:
                    [
                        new DocumentoExpedicionPdfDto(
                            Id: "0025776338|26",
                            DocumentNumber: "0025776338",
                            Fecha: new DateTime(2026, 9, 25),
                            Bultos: 5,
                            Peso: 52.30m),

                        new DocumentoExpedicionPdfDto(
                            Id: "0025776339|26",
                            DocumentNumber: "0025776339",
                            Fecha: new DateTime(2026, 9, 25),
                            Bultos: 10,
                            Peso: 112.20m)
                    ]),

                new DocumentoEnvioPdfDto(
                    Referencia: "ENVIO-002",
                    Destino: "ALMACÉN VALENCIA",
                    Direccion: "Av. Industria, 80",
                    CodigoPostal: "46980",
                    Ciudad: "Paterna",
                    CodigoPais: "ES",
                    TotalBultos: 7,
                    TotalPeso: 89.75m,
                    Expediciones:
                    [
                        new DocumentoExpedicionPdfDto(
                            Id: "0025776401|26",
                            DocumentNumber: "0025776401",
                            Fecha: new DateTime(2026, 9, 25),
                            Bultos: 3,
                            Peso: 37.25m),

                        new DocumentoExpedicionPdfDto(
                            Id: "0025776402|26",
                            DocumentNumber: "0025776402",
                            Fecha: new DateTime(2026, 9, 25),
                            Bultos: 4,
                            Peso: 52.50m)
                    ])
            ]);

        var generator = new EnviosPdfGenerator();

        // Act
        var pdf = generator.Generate(dto);

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);

        var outputDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "GeneratedPdfs");

        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            "envios-test.pdf");

        File.WriteAllBytes(outputPath, pdf);

        Assert.True(File.Exists(outputPath));

        Console.WriteLine(
            $"PDF generado en: {outputPath}");
    }

    [Fact]
    public async Task Generate_DesdeDocumentoReal_GeneraPdf()
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

        var dto =
            DocumentoEnviosPdfMapper.Map(documento);

        var pdf =
            generator.Generate(dto);

        Assert.NotEmpty(pdf);

        var outputDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "GeneratedPdfs");

        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            $"envios-{documentoId}.pdf");

        await File.WriteAllBytesAsync(
            outputPath,
            pdf);

        Console.WriteLine(
            $"PDF generado en: {outputPath}");
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