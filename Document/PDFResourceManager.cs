using QuestPDF.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace ACTA.Document;

public static class PdfResourceManager
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        RegisterFont(DocumentFonts.Regular);
        RegisterFont(DocumentFonts.Bold);
        RegisterFont(DocumentFonts.Italic);
        RegisterFont(DocumentFonts.BoldItalic);

        _initialized = true;
    }

    public static byte[] LoadImage(Uri uri)
    {
        StreamResourceInfo? resource =
            Application.GetResourceStream(uri);

        if (resource is null)
        {
            throw new FileNotFoundException(
                $"No se encontró el recurso: {uri}"
            );
        }

        using Stream source =
            resource.Stream;

        using MemoryStream destination =
            new();

        source.CopyTo(destination);

        return destination.ToArray();
    }

    private static void RegisterFont(Uri uri)
    {
        StreamResourceInfo? resource =
            Application.GetResourceStream(uri);

        if (resource is null)
        {
            throw new FileNotFoundException(
                $"No se encontró la fuente: {uri}"
            );
        }

        using Stream stream =
            resource.Stream;

        FontManager.RegisterFontFromStream(
            stream
        );
    }
}