using ACTA.Document;
using ACTA.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;

using PdfDocument =
    QuestPDF.Fluent.Document;

namespace ACTA.Services;

public sealed class PdfDocumentServiceV1 :
    IDocumentService
{
    private const int ParticipantRows = 10;

    private const float BorderWidth = 0.5f;

    public void Create(
        string filePath,
        MeetingAct meetingAct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            filePath
        );

        ArgumentNullException.ThrowIfNull(
            meetingAct
        );

        CreateDirectory(filePath);

        PdfResourceManager.Initialize();

        byte[] logo =
            PdfResourceManager.LoadImage(
                DocumentImages.Logo
            );

        PdfDocument
            .Create(document =>
            {
                document.Page(page =>
                {
                    /*
                     * Oficio Chile
                     * 21,59 x 33,02 cm
                     */
                    page.Size(
                        21.59f,
                        33.02f,
                        Unit.Centimetre
                    );

                    page.Margin(
                        1,
                        Unit.Centimetre
                    );

                    page.DefaultTextStyle(
                        style =>
                            style
                                .FontFamily(
                                    DocumentFonts.FamilyName
                                )
                                .FontSize(11)
                    );

                    page.Content()
                        .Column(column =>
                        {
                            ComposeHeader(
                                column,
                                meetingAct.Header,
                                logo
                            );

                            AddSeparator(column);

                            ComposeParticipants(
                                column,
                                meetingAct.Participants
                            );

                            AddSeparator(column);

                            ComposeSection(
                                column,
                                "MOTIVOS",
                                meetingAct.Motives
                            );

                            AddSeparator(column);

                            ComposeSection(
                                column,
                                "ACUERDOS",
                                meetingAct.Agreements
                            );

                            AddSeparator(column);

                            ComposeSection(
                                column,
                                "COMPROMISOS",
                                meetingAct.Commitments
                            );
                        });
                });
            })
            .GeneratePdf(filePath);
    }

    private static void ComposeHeader(
        ColumnDescriptor column,
        Header header,
        byte[] logo)
    {
        column.Item()
            .Row(row =>
            {
                row.RelativeItem()
                    .AlignTop()
                    .Text(header.ToString())
                    .FontSize(10)
                    .Italic();

                row.RelativeItem()
                    .AlignBottom()
                    .AlignCenter()
                    .Text("ACTA DE REUNIÓN")
                    .FontSize(12)
                    .Bold();

                row.RelativeItem()
                    .AlignRight()
                    .Width(Cm(1.54f))
                    .Height(Cm(2.00f))
                    .Image(logo)
                    .FitArea();
            });
    }

    private static void ComposeParticipants(
        ColumnDescriptor column,
        IEnumerable<Participant> participants)
    {
        List<Participant> participantList =
            participants
                .Take(ParticipantRows)
                .ToList();

        column.Item()
            .Table(table =>
            {
                table.ColumnsDefinition(
                    columns =>
                    {
                        columns.ConstantColumn(
                            Cm(5.99f)
                        );

                        columns.ConstantColumn(
                            Cm(4.75f)
                        );

                        columns.ConstantColumn(
                            Cm(2.50f)
                        );

                        columns.ConstantColumn(
                            Cm(2.75f)
                        );

                        columns.ConstantColumn(
                            Cm(3.60f)
                        );
                    }
                );

                table.Header(header =>
                {
                    AddParticipantHeaderCell(
                        header.Cell(),
                        "PARTICIPANTES"
                    );

                    AddParticipantHeaderCell(
                        header.Cell(),
                        "ROL"
                    );

                    AddParticipantHeaderCell(
                        header.Cell(),
                        "RUN"
                    );

                    AddParticipantHeaderCell(
                        header.Cell(),
                        "TELÉFONO"
                    );

                    AddParticipantHeaderCell(
                        header.Cell(),
                        "FIRMA"
                    );
                });

                foreach (
                    Participant participant
                    in participantList)
                {
                    AddParticipantCell(
                        table.Cell(),
                        participant.Name,
                        centered: false
                    );

                    AddParticipantCell(
                        table.Cell(),
                        participant.Role,
                        centered: false
                    );

                    AddParticipantCell(
                        table.Cell(),
                        participant.Run,
                        centered: true
                    );

                    AddParticipantCell(
                        table.Cell(),
                        FormatPhone(
                            participant.Phone
                        ),
                        centered: true
                    );

                    AddParticipantCell(
                        table.Cell(),
                        string.Empty,
                        centered: true
                    );
                }

                int emptyRows =
                    ParticipantRows -
                    participantList.Count;

                for (
                    int index = 0;
                    index < emptyRows;
                    index++)
                {
                    for (
                        int columnIndex = 0;
                        columnIndex < 5;
                        columnIndex++)
                    {
                        AddParticipantCell(
                            table.Cell(),
                            string.Empty,
                            centered:
                                columnIndex >= 2
                        );
                    }
                }
            });
    }

    private static void AddParticipantHeaderCell(
        IContainer container,
        string text)
    {
        container
            .Height(Cm(1))
            .Border(BorderWidth)
            .Background("#D9D9D9")
            .Padding(Cm(0.1f))
            .AlignCenter()
            .AlignMiddle()
            .Text(text)
            .FontSize(10)
            .Bold();
    }

    private static void AddParticipantCell(
        IContainer container,
        string text,
        bool centered)
    {
        IContainer cell =
            container
                .Height(Cm(1))
                .Border(BorderWidth)
                .Padding(Cm(0.1f))
                .AlignMiddle();

        if (centered)
        {
            cell = cell.AlignCenter();
        }
        else
        {
            cell = cell.AlignLeft();
        }

        cell
            .Text(text)
            .FontSize(9)
            .Bold();
    }

    private static void ComposeSection(
        ColumnDescriptor column,
        string title,
        string content)
    {
        column.Item()
            .Table(table =>
            {
                table.ColumnsDefinition(
                    columns =>
                    {
                        columns.RelativeColumn();
                    }
                );

                table.Cell()
                    .Border(BorderWidth)
                    .Background("#D9D9D9")
                    .Padding(Cm(0.1f))
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(title)
                    .FontSize(11)
                    .Bold();

                table.Cell()
                    .Border(BorderWidth)
                    .Height(Cm((float)DocumentLayout.SectionContentHeightCm))
                    .Padding(Cm(0.1f))
                    .AlignLeft()
                    .AlignTop()
                    .Text(content)
                    .FontFamily(DocumentFonts.FamilyName)
                    .FontSize(10);
            });
    }

    private static void AddSeparator(ColumnDescriptor column)
    {
        column.Item()
            .Text(" ")
            .FontFamily(DocumentFonts.FamilyName)
            .FontSize(4);
    }

    private static string FormatPhone(
        string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        return $"+56 9 {phone}";
    }

    private static float Cm(
        float centimeters)
    {
        const float pointsPerInch = 72f;
        const float centimetersPerInch = 2.54f;

        return
            centimeters /
            centimetersPerInch *
            pointsPerInch;
    }

    private static void CreateDirectory(
        string filePath)
    {
        string? directory =
            Path.GetDirectoryName(filePath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }
}