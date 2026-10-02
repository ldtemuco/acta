using ACTA.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using TableCellBottomMargin = DocumentFormat.OpenXml.Wordprocessing.BottomMargin;
using TableCellTopMargin = DocumentFormat.OpenXml.Wordprocessing.TopMargin;

namespace ACTA.Document;

public static class DocumentTableBuilder
{
    private const int ParticipantRows = 10;

    public static Table CreateParticipantsTable(IEnumerable<Participant> participants)
    {
        Table table = new();

        table.Append(
            CreateParticipantsTableProperties(),
            CreateParticipantsGrid(),
            CreateParticipantsHeaderRow()
        );

        List<Participant> participantList = participants.Take(ParticipantRows).ToList();

        foreach (Participant participant in participantList)
        {
            table.Append(CreateParticipantRow(participant));
        }

        int emptyRows = ParticipantRows - participantList.Count;

        for (int i = 0; i < emptyRows; i++)
        {
            table.Append(CreateEmptyParticipantRow());
        }

        return table;
    }

    private static TableBorders CreateNoBorders()
    {
        TopBorder top = new()
        {
            Val = BorderValues.None,
            Size = 0U
        };

        LeftBorder left = new()
        {
            Val = BorderValues.None,
            Size = 0U
        };

        BottomBorder bottom = new()
        {
            Val = BorderValues.None,
            Size = 0U
        };

        RightBorder right = new()
        {
            Val = BorderValues.None,
            Size = 0U
        };

        InsideHorizontalBorder horizontal = new()
        {
            Val = BorderValues.None,
            Size = 0U
        };

        InsideVerticalBorder vertical = new()
        {
            Val = BorderValues.None,
            Size = 0U
        };

        TableBorders borders = new();

        borders.Append(top);
        borders.Append(left);
        borders.Append(bottom);
        borders.Append(right);
        borders.Append(horizontal);
        borders.Append(vertical);

        return borders;
    }

    private static TableGrid CreateHeaderGrid()
    {
        TableGrid grid = new();

        grid.Append(
            CreateGridColumn(DocumentLayout.HeaderLeftWidth),
            CreateGridColumn(DocumentLayout.HeaderCenterWidth),
            CreateGridColumn(DocumentLayout.HeaderRightWidth)
        );

        return grid;
    }

    private static TableCellMarginDefault CreateHeaderCellMargins()
    {
        short padding = checked((short)DocumentLayout.CellPadding);

        TableCellLeftMargin left = new()
        {
            Width = padding,
            Type = TableWidthValues.Dxa
        };

        TableCellRightMargin right = new()
        {
            Width = padding,
            Type = TableWidthValues.Dxa
        };

        TableCellMarginDefault margins = new();

        margins.Append(left);
        margins.Append(right);

        return margins;
    }

    private static TableCell CreateHeaderInfoCell(Header header)
    {
        string text = header.ToString();

        Paragraph paragraph = DocumentHelpers.CreateParagraph(text,DocumentStyles.LeftParagraph(), DocumentStyles.HeaderInfo());

        return DocumentHelpers.CreateCell(paragraph, DocumentLayout.HeaderLeftWidth, CellVerticalAlignment.Top);
    }

    private static TableCell CreateHeaderTitleCell()
    {
        Paragraph paragraph = DocumentHelpers.CreateParagraph("ACTA DE REUNIÓN", DocumentStyles.CenterParagraph(), DocumentStyles.HeaderTitle());

        return DocumentHelpers.CreateCell(paragraph, DocumentLayout.HeaderCenterWidth, CellVerticalAlignment.Bottom);
    }

    private static TableCell CreateHeaderLogoCell(MainDocumentPart mainDocumentPart, Stream logoStream)
    {
        Run logoRun = DocumentHelpers.CreateImageRun(
            mainDocumentPart,
            logoStream,
            ImagePartType.Png,
            DocumentLayout.LogoWidth,
            DocumentLayout.LogoHeight,
            "Liceo del Desarrollo - Formación TP"
        );

        Paragraph paragraph = new();

        ParagraphProperties paragraphProperties = DocumentStyles.RightParagraph();

        paragraph.Append(paragraphProperties);
        paragraph.Append(logoRun);

        return DocumentHelpers.CreateCell(paragraph, DocumentLayout.HeaderRightWidth, CellVerticalAlignment.Top);
    }


    private static TableProperties CreateHeaderTableProperties()
    {
        TableWidth tableWidth = new()
        {
            Width = DocumentLayout.ContentWidth.ToString(),
            Type = TableWidthUnitValues.Dxa
        };

        TableLayout tableLayout = new()
        {
            Type = TableLayoutValues.Fixed
        };

        TableJustification justification = new()
        {
            Val = TableRowAlignmentValues.Center
        };

        TableBorders borders = CreateNoBorders();

        TableCellMarginDefault margins =
            CreateHeaderCellMargins();

        TableProperties properties = new();

        properties.Append(tableWidth);
        properties.Append(tableLayout);
        properties.Append(justification);
        properties.Append(borders);
        properties.Append(margins);

        return properties;
    }

    public static Table CreateHeaderTable(MainDocumentPart mainDocumentPart, Header header, Stream logoStream)
    {
        Table table = new();

        TableProperties properties = CreateHeaderTableProperties();

        TableGrid grid = CreateHeaderGrid();

        TableRow row = new();

        row.Append(
            CreateHeaderInfoCell(header),
            CreateHeaderTitleCell(),
            CreateHeaderLogoCell(mainDocumentPart, logoStream)
        );

        table.Append(properties);
        table.Append(grid);
        table.Append(row);

        return table;
    }

    public static Table CreateSectionTable(string title, string content)
    {
        Table table = new();

        table.Append(
            CreateSectionTableProperties(),
            CreateSectionGrid(),
            CreateSectionTitleRow(title),
            CreateSectionContentRow(content)
        );

        return table;
    }

    private static TableProperties CreateParticipantsTableProperties()
    {
        TableWidth tableWidth = new()
        {
            Width = DocumentLayout.ContentWidth.ToString(),
            Type = TableWidthUnitValues.Dxa
        };

        TableLayout tableLayout = new()
        {
            Type = TableLayoutValues.Fixed
        };

        TableCellMarginDefault cellMargins = CreateCellMargins();

        TableProperties properties = new();

        TableBorders borders = DocumentStyles.TableBorders();

        properties.Append(tableWidth);
        properties.Append(tableLayout);
        properties.Append(cellMargins);
        properties.Append(borders);

        return properties;
    }

    private static TableGrid CreateParticipantsGrid()
    {
        TableGrid grid = new();

        grid.Append(
            CreateGridColumn(DocumentLayout.ParticipantNameWidth),
            CreateGridColumn(DocumentLayout.ParticipantRoleWidth),
            CreateGridColumn(DocumentLayout.ParticipantRunWidth),
            CreateGridColumn(DocumentLayout.ParticipantPhoneWidth),
            CreateGridColumn(DocumentLayout.ParticipantSignatureWidth)
        );

        return grid;
    }

    private static TableRow CreateParticipantsHeaderRow()
    {
        TableRow row = DocumentHelpers.CreateRow(DocumentLayout.ParticipantRowHeight, exactHeight: true);

        row.Append(
            CreateHeaderCell("PARTICIPANTES", DocumentLayout.ParticipantNameWidth),
            CreateHeaderCell("ROL", DocumentLayout.ParticipantRoleWidth),
            CreateHeaderCell("RUN", DocumentLayout.ParticipantRunWidth),
            CreateHeaderCell("TELÉFONO", DocumentLayout.ParticipantPhoneWidth),
            CreateHeaderCell("FIRMA", DocumentLayout.ParticipantSignatureWidth)
        );

        return row;
    }


    private static TableRow CreateParticipantRow(Participant participant)
    {
        TableRow row = DocumentHelpers.CreateRow(DocumentLayout.ParticipantRowHeight, exactHeight: true);

        row.Append(
            CreateParticipantCell(participant.Name, DocumentLayout.ParticipantNameWidth, false),
            CreateParticipantCell(participant.Role, DocumentLayout.ParticipantRoleWidth, false),
            CreateParticipantCell(participant.Run, DocumentLayout.ParticipantRunWidth, true),
            CreateParticipantCell(DocumentHelpers.FormatPhone(participant.Phone), DocumentLayout.ParticipantPhoneWidth, true),
            CreateParticipantCell(string.Empty, DocumentLayout.ParticipantSignatureWidth, true)
        );

        return row;
    }

    private static TableRow CreateEmptyParticipantRow()
    {
        TableRow row = DocumentHelpers.CreateRow(DocumentLayout.ParticipantRowHeight, exactHeight: true);

        row.Append(
            CreateParticipantCell(string.Empty, DocumentLayout.ParticipantNameWidth, false),
            CreateParticipantCell(string.Empty, DocumentLayout.ParticipantRoleWidth, false),
            CreateParticipantCell(string.Empty, DocumentLayout.ParticipantRunWidth, true),
            CreateParticipantCell(string.Empty, DocumentLayout.ParticipantPhoneWidth, true),
            CreateParticipantCell(string.Empty, DocumentLayout.ParticipantSignatureWidth, true)
        );

        return row;
    }

    private static TableCell CreateHeaderCell(string text, int width)
    {
        Paragraph paragraph = DocumentHelpers.CreateParagraph(text, DocumentStyles.CenterParagraph(), DocumentStyles.TableHeader());

        TableCell cell = DocumentHelpers.CreateCell(paragraph, width);

        TableCellProperties properties = cell.GetFirstChild<TableCellProperties>()!;

        Shading shading = new()
        {
            Val = ShadingPatternValues.Clear,
            Fill = "D9D9D9"
        };

        properties.Append(shading);

        return cell;
    }

    private static TableCell CreateParticipantCell(string text, int width, bool centered)
    {
        ParagraphProperties paragraphProperties = centered ? DocumentStyles.CenterParagraph() : DocumentStyles.LeftParagraph();

        Paragraph paragraph = DocumentHelpers.CreateParagraph(text, paragraphProperties, DocumentStyles.ParticipantText());

        return DocumentHelpers.CreateCell(paragraph, width);
    }

    private static TableProperties CreateSectionTableProperties()
    {
        TableWidth tableWidth = new()
        {
            Width = DocumentLayout.ContentWidth.ToString(),
            Type = TableWidthUnitValues.Dxa
        };

        TableLayout tableLayout = new()
        {
            Type = TableLayoutValues.Fixed
        };

        TableCellMarginDefault cellMargins = CreateCellMargins();

        TableProperties properties = new();

        TableBorders borders = DocumentStyles.TableBorders();

        properties.Append(tableWidth);
        properties.Append(tableLayout);
        properties.Append(cellMargins);
        properties.Append(borders);

        return properties;
    }

    private static TableGrid CreateSectionGrid()
    {
        TableGrid grid = new();

        grid.Append(
            CreateGridColumn(DocumentLayout.ContentWidth)
        );

        return grid;
    }

    private static TableRow CreateSectionTitleRow(string title)
    {
        Paragraph paragraph = DocumentHelpers.CreateParagraph(title, DocumentStyles.CenterParagraph(), DocumentStyles.SectionTitle());

        TableCell cell = DocumentHelpers.CreateCell(paragraph, DocumentLayout.ContentWidth);

        TableCellProperties properties = cell.GetFirstChild<TableCellProperties>()!;

        Shading shading = new()
        {
            Val = ShadingPatternValues.Clear,
            Fill = "D9D9D9"
        };

        properties.Append(shading);

        TableRow row = new();
        row.Append(cell);

        return row;
    }

    private static TableRow CreateSectionContentRow(string content)
    {
        Paragraph paragraph = DocumentHelpers.CreateParagraph(content, DocumentStyles.LeftParagraph(), DocumentStyles.BodyText());

        TableCell cell = DocumentHelpers.CreateCell(paragraph, DocumentLayout.ContentWidth, CellVerticalAlignment.Top);

        TableRow row = new();
        row.Append(cell);

        return row;
    }

    private static GridColumn CreateGridColumn(int width)
    {
        return new GridColumn
        {
            Width = width.ToString()
        };
    }

    private static TableCellMarginDefault CreateCellMargins()
    {
        string verticalPadding = DocumentLayout.CellPadding.ToString();

        short horizontalPadding = checked((short)DocumentLayout.CellPadding);

        TableCellTopMargin top = new()
        {
            Width = verticalPadding,
            Type = TableWidthUnitValues.Dxa
        };

        TableCellLeftMargin left = new()
        {
            Width = horizontalPadding,
            Type = TableWidthValues.Dxa
        };

        TableCellBottomMargin bottom = new()
        {
            Width = verticalPadding,
            Type = TableWidthUnitValues.Dxa
        };

        TableCellRightMargin right = new()
        {
            Width = horizontalPadding,
            Type = TableWidthValues.Dxa
        };

        TableCellMarginDefault margins = new();

        margins.Append(top);
        margins.Append(left);
        margins.Append(bottom);
        margins.Append(right);

        return margins;
    }
}