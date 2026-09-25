using Dtd.Application.Documentos.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Dtd.Infrastructure.Pdf;

internal sealed class EnviosPdfGenerator : IEnviosPdfGenerator
{
    public byte[] Generate(DocumentoEnviosPdfDto documento)
    {
        ArgumentNullException.ThrowIfNull(documento);

        PdfFontConfiguration.Configure();

        var pdfDocument = CrearDocumento(documento);

        var renderer = new PdfDocumentRenderer
        {
            Document = pdfDocument
        };

        renderer.RenderDocument();

        using var stream = new MemoryStream();

        renderer.PdfDocument.Save(stream, closeStream: false);

        return stream.ToArray();
    }
    private static Document CrearDocumento(
        DocumentoEnviosPdfDto documento)
    {
        var document = new Document
        {
            Info =
            {
                Title = $"Envíos - {documento.Referencia}",
                Subject = "Relación de envíos y expediciones",
                Author = "Grupo Soledad"
            }
        };

        ConfigurarEstilos(document);

        var section = document.AddSection();

        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

        AddCabecera(section, documento);

        foreach (var envio in documento.Envios)
        {
            AddEnvio(section, envio);
        }

        AddPiePagina(section);

        return document;
    }

    private static void ConfigurarEstilos(
        Document document)
    {
        var normal = document.Styles[StyleNames.Normal];

        normal.Font.Name = "Liberation Sans";
        normal.Font.Size = 9;

        var heading1 = document.Styles[StyleNames.Heading1];

        heading1.Font.Name = "Liberation Sans";
        heading1.Font.Size = 16;
        heading1.Font.Bold = true;
        heading1.ParagraphFormat.SpaceAfter = Unit.FromPoint(8);

        var heading2 = document.Styles[StyleNames.Heading2];

        heading2.Font.Name = "Liberation Sans";
        heading2.Font.Size = 12;
        heading2.Font.Bold = true;
        heading2.ParagraphFormat.SpaceBefore = Unit.FromPoint(12);
        heading2.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);
    }

    private static void AddCabecera(
        Section section,
        DocumentoEnviosPdfDto documento)
    {
        var titulo = section.AddParagraph(
            "RELACIÓN DE ENVÍOS");

        titulo.Style = StyleNames.Heading1;

        var table = section.AddTable();

        table.Borders.Width = 0;

        table.AddColumn(Unit.FromCentimeter(4));
        table.AddColumn(Unit.FromCentimeter(12.5));

        AddDatoCabecera(
            table,
            "Referencia:",
            documento.Referencia);

        AddDatoCabecera(
            table,
            "Empresa:",
            documento.Empresa);

        AddDatoCabecera(
            table,
            "Fecha:",
            documento.FechaCreacion.ToString("dd/MM/yyyy HH:mm"));

        section.AddParagraph()
            .Format.SpaceAfter = Unit.FromPoint(8);
    }

    private static void AddDatoCabecera(
        Table table,
        string etiqueta,
        string valor)
    {
        var row = table.AddRow();

        var label = row.Cells[0].AddParagraph();
        label.AddFormattedText(
            etiqueta,
            TextFormat.Bold);

        row.Cells[1]
            .AddParagraph(valor ?? string.Empty);
    }

    private static void AddEnvio(
        Section section,
        DocumentoEnvioPdfDto envio)
    {
        var titulo = section.AddParagraph(
            $"Envío {envio.Referencia}");

        titulo.Style = StyleNames.Heading2;

        AddDestino(section, envio);

        AddResumenEnvio(section, envio);

        AddExpediciones(section, envio.Expediciones);

        section.AddParagraph()
            .Format.SpaceAfter = Unit.FromPoint(8);
    }

    private static void AddDestino(
        Section section,
        DocumentoEnvioPdfDto envio)
    {
        var table = section.AddTable();

        table.Borders.Width = 0;

        table.AddColumn(Unit.FromCentimeter(3));
        table.AddColumn(Unit.FromCentimeter(13.5));

        AddDato(
            table,
            "Destino",
            envio.Destino);

        AddDato(
            table,
            "Dirección",
            envio.Direccion);

        var localidad = ConstruirLocalidad(envio);

        if (!string.IsNullOrWhiteSpace(localidad))
        {
            AddDato(
                table,
                "Localidad",
                localidad);
        }

        if (!string.IsNullOrWhiteSpace(envio.CodigoPais))
        {
            AddDato(
                table,
                "País",
                envio.CodigoPais!);
        }

        section.AddParagraph()
            .Format.SpaceAfter = Unit.FromPoint(5);
    }

    private static string ConstruirLocalidad(
        DocumentoEnvioPdfDto envio)
    {
        return string.Join(
            " ",
            new[]
            {
                envio.CodigoPostal,
                envio.Ciudad
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static void AddDato(
        Table table,
        string etiqueta,
        string valor)
    {
        var row = table.AddRow();

        var label = row.Cells[0].AddParagraph();

        label.AddFormattedText(
            $"{etiqueta}:",
            TextFormat.Bold);

        row.Cells[1]
            .AddParagraph(valor ?? string.Empty);
    }

    private static void AddResumenEnvio(
        Section section,
        DocumentoEnvioPdfDto envio)
    {
        var paragraph = section.AddParagraph();

        paragraph.Format.SpaceBefore = Unit.FromPoint(4);
        paragraph.Format.SpaceAfter = Unit.FromPoint(6);

        paragraph.AddFormattedText(
            "Total bultos: ",
            TextFormat.Bold);

        paragraph.AddText(
            envio.TotalBultos.ToString());

        paragraph.AddText("    ");

        paragraph.AddFormattedText(
            "Peso total: ",
            TextFormat.Bold);

        paragraph.AddText(
            $"{envio.TotalPeso:N2} kg");
    }

    private static void AddExpediciones(
    Section section,
    IReadOnlyCollection<DocumentoExpedicionPdfDto> expediciones)
    {
        var table = section.AddTable();

        table.Borders.Width = 0.5;

        table.AddColumn(Unit.FromCentimeter(4.2)); // Id ERP
        table.AddColumn(Unit.FromCentimeter(3.8)); // Nº documento
        table.AddColumn(Unit.FromCentimeter(3));   // Fecha
        table.AddColumn(Unit.FromCentimeter(2));   // Bultos
        table.AddColumn(Unit.FromCentimeter(3.5)); // Peso

        var header = table.AddRow();

        header.HeadingFormat = true;
        header.Format.Font.Bold = true;

        header.Cells[0]
            .AddParagraph("Id expedición");

        header.Cells[1]
            .AddParagraph("Nº documento");

        header.Cells[2]
            .AddParagraph("Fecha");

        header.Cells[3]
            .AddParagraph("Bultos");

        header.Cells[4]
            .AddParagraph("Peso");

        foreach (var expedicion in expediciones)
        {
            var row = table.AddRow();

            row.Cells[0]
                .AddParagraph(expedicion.Id);

            row.Cells[1]
                .AddParagraph(
                    expedicion.DocumentNumber ?? string.Empty);

            row.Cells[2]
                .AddParagraph(
                    expedicion.Fecha.ToString("dd/MM/yyyy"));

            row.Cells[3]
                .AddParagraph(
                    expedicion.Bultos.ToString());

            row.Cells[4]
                .AddParagraph(
                    $"{expedicion.Peso:N2} kg");
        }
    }
    private static void AddPiePagina(
        Section section)
    {
        var paragraph =
            section.Footers.Primary.AddParagraph();

        paragraph.Format.Alignment =
            ParagraphAlignment.Center;

        paragraph.Format.Font.Size = 8;

        paragraph.AddText("Página ");

        paragraph.AddPageField();

        paragraph.AddText(" de ");

        paragraph.AddNumPagesField();
    }
}