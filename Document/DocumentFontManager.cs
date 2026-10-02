using ACTA.Document;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Resources;

using WordFont = DocumentFormat.OpenXml.Wordprocessing.Font;

namespace ACTA.Documents;

public static class DocumentFontManager
{
    public static void EmbedFonts(
        MainDocumentPart mainDocumentPart)
    {
        ArgumentNullException.ThrowIfNull(mainDocumentPart);

        FontTablePart fontTablePart =
            GetOrCreateFontTablePart(mainDocumentPart);

        RemoveExistingFont(fontTablePart);

        WordFont font = new()
        {
            Name = DocumentFonts.FamilyName
        };

        EmbedRegularFont regular =
            CreateRegularFont(
                fontTablePart,
                DocumentFonts.Regular
            );

        EmbedBoldFont bold =
            CreateBoldFont(
                fontTablePart,
                DocumentFonts.Bold
            );

        EmbedItalicFont italic =
            CreateItalicFont(
                fontTablePart,
                DocumentFonts.Italic
            );

        EmbedBoldItalicFont boldItalic =
            CreateBoldItalicFont(
                fontTablePart,
                DocumentFonts.BoldItalic
            );

        font.Append(regular);
        font.Append(bold);
        font.Append(italic);
        font.Append(boldItalic);

        fontTablePart.Fonts!.Append(font);
        fontTablePart.Fonts.Save();

        ConfigureEmbeddingSettings(mainDocumentPart);
    }

    private static FontTablePart GetOrCreateFontTablePart(
        MainDocumentPart mainDocumentPart)
    {
        FontTablePart? fontTablePart =
            mainDocumentPart.FontTablePart;

        if (fontTablePart is null)
        {
            fontTablePart =
                mainDocumentPart.AddNewPart<FontTablePart>();

            fontTablePart.Fonts = new Fonts();
        }
        else if (fontTablePart.Fonts is null)
        {
            fontTablePart.Fonts = new Fonts();
        }

        return fontTablePart;
    }

    private static EmbedRegularFont CreateRegularFont(
        FontTablePart fontTablePart,
        Uri fontUri)
    {
        EmbeddedFont embeddedFont =
            CreateEmbeddedFont(
                fontTablePart,
                fontUri
            );

        return new EmbedRegularFont
        {
            Id = embeddedFont.RelationshipId,
            FontKey = embeddedFont.FontKey
        };
    }

    private static EmbedBoldFont CreateBoldFont(
        FontTablePart fontTablePart,
        Uri fontUri)
    {
        EmbeddedFont embeddedFont =
            CreateEmbeddedFont(
                fontTablePart,
                fontUri
            );

        return new EmbedBoldFont
        {
            Id = embeddedFont.RelationshipId,
            FontKey = embeddedFont.FontKey
        };
    }

    private static EmbedItalicFont CreateItalicFont(
        FontTablePart fontTablePart,
        Uri fontUri)
    {
        EmbeddedFont embeddedFont =
            CreateEmbeddedFont(
                fontTablePart,
                fontUri
            );

        return new EmbedItalicFont
        {
            Id = embeddedFont.RelationshipId,
            FontKey = embeddedFont.FontKey
        };
    }

    private static EmbedBoldItalicFont CreateBoldItalicFont(
        FontTablePart fontTablePart,
        Uri fontUri)
    {
        EmbeddedFont embeddedFont =
            CreateEmbeddedFont(
                fontTablePart,
                fontUri
            );

        return new EmbedBoldItalicFont
        {
            Id = embeddedFont.RelationshipId,
            FontKey = embeddedFont.FontKey
        };
    }

    private static EmbeddedFont CreateEmbeddedFont(
        FontTablePart fontTablePart,
        Uri fontUri)
    {
        byte[] fontData =
            ReadFontResource(fontUri);

        Guid fontKey = Guid.NewGuid();

        byte[] obfuscatedFont =
            ObfuscateFont(
                fontData,
                fontKey
            );

        FontPart fontPart =
            fontTablePart.AddFontPart(
                FontPartType.FontOdttf
            );

        using MemoryStream stream = new(
            obfuscatedFont,
            writable: false
        );

        fontPart.FeedData(stream);

        string relationshipId =
            fontTablePart.GetIdOfPart(fontPart);

        string formattedFontKey =
            fontKey.ToString("B")
                   .ToUpperInvariant();

        return new EmbeddedFont(
            relationshipId,
            formattedFontKey
        );
    }

    private static byte[] ReadFontResource(
        Uri fontUri)
    {
        StreamResourceInfo? resource =
            Application.GetResourceStream(fontUri);

        if (resource is null)
        {
            throw new FileNotFoundException(
                $"No se encontró la fuente: {fontUri}"
            );
        }

        using Stream source = resource.Stream;
        using MemoryStream destination = new();

        source.CopyTo(destination);

        return destination.ToArray();
    }

    private static byte[] ObfuscateFont(
        byte[] fontData,
        Guid fontKey)
    {
        if (fontData.Length < 32)
        {
            throw new InvalidDataException(
                "El archivo de fuente no contiene suficientes datos."
            );
        }

        byte[] result =
            (byte[])fontData.Clone();

        byte[] key =
            CreateObfuscationKey(fontKey);

        for (int i = 0; i < 32; i++)
        {
            result[i] ^= key[i % 16];
        }

        return result;
    }

    private static byte[] CreateObfuscationKey(
        Guid fontKey)
    {
        string guid =
            fontKey.ToString("N");

        byte[] key = new byte[16];

        for (int i = 0; i < key.Length; i++)
        {
            string hex =
                guid.Substring(i * 2, 2);

            key[i] =
                Convert.ToByte(hex, 16);
        }

        Array.Reverse(key);

        return key;
    }

    private static void RemoveExistingFont(
        FontTablePart fontTablePart)
    {
        WordFont? existingFont =
            fontTablePart.Fonts?
                .Elements<WordFont>()
                .FirstOrDefault(
                    font =>
                        string.Equals(
                            font.Name?.Value,
                            DocumentFonts.FamilyName,
                            StringComparison.OrdinalIgnoreCase
                        )
                );

        existingFont?.Remove();
    }

    private static void ConfigureEmbeddingSettings(
        MainDocumentPart mainDocumentPart)
    {
        DocumentSettingsPart? settingsPart =
            mainDocumentPart.DocumentSettingsPart;

        if (settingsPart is null)
        {
            settingsPart =
                mainDocumentPart
                    .AddNewPart<DocumentSettingsPart>();

            settingsPart.Settings =
                new Settings();
        }
        else if (settingsPart.Settings is null)
        {
            settingsPart.Settings =
                new Settings();
        }

        Settings settings =
            settingsPart.Settings;

        EmbedTrueTypeFonts? embedTrueTypeFonts =
            settings
                .Elements<EmbedTrueTypeFonts>()
                .FirstOrDefault();

        if (embedTrueTypeFonts is null)
        {
            embedTrueTypeFonts =
                new EmbedTrueTypeFonts();

            settings.Append(embedTrueTypeFonts);
        }

        embedTrueTypeFonts.Val =
            new OnOffValue(true);

        settings.Save();
    }

    private readonly record struct EmbeddedFont(
        string RelationshipId,
        string FontKey
    );
}