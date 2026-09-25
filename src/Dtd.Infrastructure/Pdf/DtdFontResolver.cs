using PdfSharp.Fonts;

namespace Dtd.Infrastructure.Pdf;

public sealed class DtdFontResolver : IFontResolver
{
    private const string Regular = "LiberationSans-Regular";
    private const string Bold = "LiberationSans-Bold";
    private const string Italic = "LiberationSans-Italic";
    private const string BoldItalic = "LiberationSans-BoldItalic";

    public FontResolverInfo ResolveTypeface(
        string familyName,
        bool isBold,
        bool isItalic)
    {
        if (isBold && isItalic)
        {
            return new FontResolverInfo(BoldItalic);
        }

        if (isBold)
        {
            return new FontResolverInfo(Bold);
        }

        if (isItalic)
        {
            return new FontResolverInfo(Italic);
        }

        return new FontResolverInfo(Regular);
    }

    public byte[] GetFont(string faceName)
    {
        var resourceName = faceName switch
        {
            Regular =>
                "Dtd.Infrastructure.Fonts.LiberationSans-Regular.ttf",

            Bold =>
                "Dtd.Infrastructure.Fonts.LiberationSans-Bold.ttf",

            Italic =>
                "Dtd.Infrastructure.Fonts.LiberationSans-Italic.ttf",

            BoldItalic =>
                "Dtd.Infrastructure.Fonts.LiberationSans-BoldItalic.ttf",

            _ => throw new InvalidOperationException(
                $"Fuente no soportada: {faceName}")
        };

        var assembly = typeof(DtdFontResolver).Assembly;

        using var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"No se encuentra la fuente embebida '{resourceName}'.");

        using var memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
}