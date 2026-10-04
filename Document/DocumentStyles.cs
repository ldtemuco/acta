using DocumentFormat.OpenXml.Wordprocessing;

namespace ACTA.Document;

public static class DocumentStyles
{
    // Open XML expresa el tamaño en medios puntos:
    // 18 = 9 pt
    // 20 = 10 pt
    // 22 = 11 pt
    // 24 = 12 pt
    public static RunProperties HeaderInfo()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "20"
        };

        Italic italic = new();

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);
        properties.Append(italic);

        return properties;
    }

    public static RunProperties HeaderTitle()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "24"
        };

        Bold bold = new();

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);
        properties.Append(bold);

        return properties;
    }

    public static RunProperties TableHeader()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "20"
        };

        Bold bold = new();

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);
        properties.Append(bold);

        return properties;
    }

    public static RunProperties ParticipantText()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "18"
        };

        Bold bold = new();

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);
        properties.Append(bold);

        return properties;
    }

    public static RunProperties SectionTitle()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "22"
        };

        Bold bold = new();

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);
        properties.Append(bold);

        return properties;
    }

    public static RunProperties BodyText()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "20"
        };

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);

        return properties;
    }

    public static RunProperties SeparatorText()
    {
        RunFonts fonts = CreateFonts();

        FontSize fontSize = new()
        {
            Val = "8" // 4 pt
        };

        RunProperties properties = new();

        properties.Append(fonts);
        properties.Append(fontSize);

        return properties;
    }


    public static ParagraphProperties LeftParagraph()
    {
        return CreateParagraphProperties(JustificationValues.Left);
    }

    public static ParagraphProperties CenterParagraph()
    {
        return CreateParagraphProperties(JustificationValues.Center);
    }

    public static ParagraphProperties RightParagraph()
    {
        return CreateParagraphProperties(JustificationValues.Right);
    }

    private static RunFonts CreateFonts()
    {
        return new RunFonts
        {
            Ascii = DocumentFonts.FamilyName,
            HighAnsi = DocumentFonts.FamilyName,
            ComplexScript = DocumentFonts.FamilyName
        };
    }

    private static ParagraphProperties CreateParagraphProperties(JustificationValues alignment)
    {
        Justification justification = new()
        {
            Val = alignment
        };

        SpacingBetweenLines spacing = new()
        {
            Before = "0",
            After = "0",
            Line = "240",
            LineRule = LineSpacingRuleValues.Auto
        };

        ParagraphProperties properties = new();

        properties.Append(justification);
        properties.Append(spacing);

        return properties;
    }
       
    public static TableBorders TableBorders()
    {
        TopBorder top = new()
        {
            Val = BorderValues.Single,
            Size = 4U,
            Color = "auto"
        };

        LeftBorder left = new()
        {
            Val = BorderValues.Single,
            Size = 4U,
            Color = "auto"
        };

        BottomBorder bottom = new()
        {
            Val = BorderValues.Single,
            Size = 4U,
            Color = "auto"
        };

        RightBorder right = new()
        {
            Val = BorderValues.Single,
            Size = 4U,
            Color = "auto"
        };

        InsideHorizontalBorder insideHorizontal = new()
        {
            Val = BorderValues.Single,
            Size = 4U,
            Color = "auto"
        };

        InsideVerticalBorder insideVertical = new()
        {
            Val = BorderValues.Single,
            Size = 4U,
            Color = "auto"
        };

        TableBorders borders = new();

        borders.Append(top);
        borders.Append(left);
        borders.Append(bottom);
        borders.Append(right);
        borders.Append(insideHorizontal);
        borders.Append(insideVertical);

        return borders;
    }
}