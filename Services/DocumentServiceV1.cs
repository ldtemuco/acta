using ACTA.Document;
using ACTA.Documents;
using ACTA.Models;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using System.IO;
using System.Windows;
using System.Windows.Resources;
using MicrosoftWordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;


namespace ACTA.Services;

public class DocumentServiceV1 : IDocumentService
{
    public void Create(string filePath, MeetingAct meetingAct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(meetingAct);

        CreateDirectory(filePath);

        using WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

        MainDocumentPart mainDocumentPart = wordDocument.AddMainDocumentPart();

        mainDocumentPart.Document = new MicrosoftWordDocument();

        DocumentFontManager.EmbedFonts(mainDocumentPart);

        Body body = new();

        mainDocumentPart.Document.Append(body);

        using Stream logoStream = GetLogoStream();

        AppendHeader(
            body,
            mainDocumentPart,
            meetingAct.Header,
            logoStream
        );

        AppendSeparator(body);

        AppendParticipants(body, meetingAct.Participants);

        AppendSeparator(body);

        AppendSections(body, meetingAct);

        AppendSectionProperties(body);
        mainDocumentPart.Document.Save();
    }

    private static void AppendHeader(Body body, MainDocumentPart mainDocumentPart, Header header, Stream logoStream)
    {
        Table headerTable = DocumentTableBuilder.CreateHeaderTable(mainDocumentPart, header, logoStream);
        body.Append(headerTable);
    }

    private static void AppendParticipants(Body body, IEnumerable<Participant> participants)
    {
        Table participantsTable =DocumentTableBuilder.CreateParticipantsTable(participants);
        body.Append(participantsTable);
    }

    private static void AppendSections(Body body, MeetingAct meetingAct)
    {
        Table motivesTable = DocumentTableBuilder.CreateSectionTable("MOTIVOS", meetingAct.Motives);
        Table agreementsTable = DocumentTableBuilder.CreateSectionTable("ACUERDOS", meetingAct.Agreements);
        Table commitmentsTable = DocumentTableBuilder.CreateSectionTable("COMPROMISOS",meetingAct.Commitments);

        body.Append(motivesTable);

        AppendSeparator(body);

        body.Append(agreementsTable);

        AppendSeparator(body);

        body.Append(commitmentsTable);
    }

    private static void AppendSeparator(
        Body body)
    {
        Paragraph separator =
            CreateSeparatorParagraph();

        body.Append(separator);
    }

    private static Paragraph CreateSeparatorParagraph()
    {
        SpacingBetweenLines spacing = new()
        {
            Before = "0",
            After = "0",
            Line = "259",
            LineRule = LineSpacingRuleValues.Auto
        };

        ParagraphProperties properties = new();

        properties.Append(spacing);

        Paragraph paragraph = new();

        paragraph.Append(properties);

        return paragraph;
    }

    private static void AppendSectionProperties(Body body)
    {
        PageSize pageSize = new()
        {
            Width = new UInt32Value((uint)DocumentLayout.PageWidth),
            Height = new UInt32Value((uint)DocumentLayout.PageHeight)
        };

        PageMargin pageMargin = new()
        {
            Top = new Int32Value(DocumentLayout.MarginTop),
            Right = new UInt32Value((uint)DocumentLayout.MarginRight),
            Bottom = new Int32Value(DocumentLayout.MarginBottom),
            Left = new UInt32Value((uint)DocumentLayout.MarginLeft),
            Gutter = new UInt32Value(0U)
        };

        SectionProperties sectionProperties = new();

        sectionProperties.Append(pageSize);
        sectionProperties.Append(pageMargin);

        body.Append(sectionProperties);
    }

    private static Stream GetLogoStream()
    {
        StreamResourceInfo? resource = Application.GetResourceStream(DocumentImages.Logo);

        if (resource is null)
        {
            throw new FileNotFoundException("No se encontró el logo incrustado en los recursos.");
        }

        return resource.Stream;
    }

    private static void CreateDirectory(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }
}