using PdfSharp.Fonts;

namespace Dtd.Infrastructure.Pdf;

public static class PdfFontConfiguration
{
    private static readonly object Sync = new();
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
        {
            return;
        }

        lock (Sync)
        {
            if (_configured)
            {
                return;
            }

            GlobalFontSettings.FontResolver =
                new DtdFontResolver();

            _configured = true;
        }
    }
}