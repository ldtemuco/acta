using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.IO;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace ACTA.Document;

public enum CellVerticalAlignment
{
    Top,
    Center,
    Bottom
}

public static class DocumentHelpers
{

    public static Run CreateImageRun(MainDocumentPart mainDocumentPart, Stream imageStream, PartTypeInfo imageType, long width, long height, string imageName)
    {
        ImagePart imagePart = mainDocumentPart.AddImagePart(imageType);

        imagePart.FeedData(imageStream);

        string relationshipId = mainDocumentPart.GetIdOfPart(imagePart);

        DW.Extent extent = new()
        {
            Cx = width,
            Cy = height
        };

        DW.DocProperties documentProperties = new()
        {
            Id = 1U,
            Name = imageName
        };

        A.GraphicFrameLocks graphicFrameLocks = new()
        {
            NoChangeAspect = true
        };

        DW.NonVisualGraphicFrameDrawingProperties nonVisualGraphicFrameProperties = new();

        nonVisualGraphicFrameProperties.Append(graphicFrameLocks);

        PIC.NonVisualDrawingProperties nonVisualDrawingProperties = new()
        {
            Id = 0U,
            Name = imageName
        };

        PIC.NonVisualPictureDrawingProperties nonVisualPictureDrawingProperties = new();

        PIC.NonVisualPictureProperties nonVisualPictureProperties = new();

        nonVisualPictureProperties.Append(
            nonVisualDrawingProperties
        );

        nonVisualPictureProperties.Append(
            nonVisualPictureDrawingProperties
        );

        A.Blip blip = new()
        {
            Embed = relationshipId
        };

        A.FillRectangle fillRectangle = new();

        A.Stretch stretch = new();
        stretch.Append(fillRectangle);

        PIC.BlipFill blipFill = new();

        blipFill.Append(blip);
        blipFill.Append(stretch);

        A.Offset offset = new()
        {
            X = 0L,
            Y = 0L
        };

        A.Extents extents = new()
        {
            Cx = width,
            Cy = height
        };

        A.Transform2D transform = new();

        transform.Append(offset);
        transform.Append(extents);

        A.AdjustValueList adjustValueList = new();

        A.PresetGeometry geometry = new()
        {
            Preset = A.ShapeTypeValues.Rectangle
        };

        geometry.Append(adjustValueList);

        PIC.ShapeProperties shapeProperties = new();

        shapeProperties.Append(transform);
        shapeProperties.Append(geometry);

        PIC.Picture picture = new();

        picture.Append(nonVisualPictureProperties);
        picture.Append(blipFill);
        picture.Append(shapeProperties);

        A.GraphicData graphicData = new()
        {
            Uri =
                "http://schemas.openxmlformats.org/drawingml/2006/picture"
        };

        graphicData.Append(picture);

        A.Graphic graphic = new();
        graphic.Append(graphicData);

        DW.Inline inline = new()
        {
            DistanceFromTop = 0U,
            DistanceFromBottom = 0U,
            DistanceFromLeft = 0U,
            DistanceFromRight = 0U
        };

        inline.Append(extent);
        inline.Append(documentProperties);
        inline.Append(nonVisualGraphicFrameProperties);
        inline.Append(graphic);

        Drawing drawing = new();
        drawing.Append(inline);

        Run run = new();
        run.Append(drawing);

        return run;
    }
    public static Text CreateText(string text)
    {
        return new Text(text)
        {
            Space = SpaceProcessingModeValues.Preserve
        };
    }

    public static Run CreateRun(string text, RunProperties? properties = null)
    {
        Run run = new();

        if (properties is not null)
        {
            run.Append((RunProperties)properties.CloneNode(true));
        }

        AppendText(run, text);

        return run;
    }

    public static Paragraph CreateParagraph(string text = "", ParagraphProperties? paragraphProperties = null, RunProperties? runProperties = null)
    {
        Paragraph paragraph = new();

        if (paragraphProperties is not null)
        {
            paragraph.Append((ParagraphProperties)paragraphProperties.CloneNode(true));
        }

        paragraph.Append(CreateRun(text, runProperties));

        return paragraph;
    }

    public static TableCell CreateCell(Paragraph paragraph, int width, CellVerticalAlignment verticalAlignment = CellVerticalAlignment.Center)
    {
        TableCellWidth cellWidth = new()
        {
            Width = width.ToString(CultureInfo.InvariantCulture),
            Type = TableWidthUnitValues.Dxa
        };

        TableCellVerticalAlignment cellAlignment = new()
        {
            Val = GetVerticalAlignment(verticalAlignment)
        };

        TableCellProperties properties = new();

        properties.Append(cellWidth);
        properties.Append(cellAlignment);

        TableCell cell = new();

        cell.Append(properties);
        cell.Append(paragraph);

        return cell;
    }

    private static TableVerticalAlignmentValues GetVerticalAlignment(CellVerticalAlignment alignment)
    {
        return alignment switch
        {
            CellVerticalAlignment.Top => TableVerticalAlignmentValues.Top,
            CellVerticalAlignment.Center => TableVerticalAlignmentValues.Center,
            CellVerticalAlignment.Bottom => TableVerticalAlignmentValues.Bottom,
            _ => TableVerticalAlignmentValues.Center
        };
    }

    public static TableRow CreateRow(int? height = null, bool exactHeight = false, bool preventSplit = true)
    {
        TableRow row = new();
        TableRowProperties properties = new();

        if (preventSplit)
        {
            properties.Append(new CantSplit());
        }

        if (height.HasValue)
        {
            TableRowHeight rowHeight = new()
            {
                Val = new UInt32Value((uint)height.Value),
                HeightType = exactHeight ? HeightRuleValues.Exact : HeightRuleValues.AtLeast
            };

            properties.Append(rowHeight);
        }

        row.Append(properties);

        return row;
    }


    public static string FormatPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        return $"+56 9 {phone}";
    }

    public static string FormatDate(DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public static string FormatTime(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm'hrs.'", CultureInfo.InvariantCulture);
    }

    private static void AppendText(Run run, string text)
    {
        string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                run.Append(new Break());
            }

            run.Append(CreateText(lines[i]));
        }
    }
}