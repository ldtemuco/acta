using ACTA.Data;
using ACTA.Models;
using ACTA.Services;
using ACTA.Views;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace ACTA;

public partial class MainWindow : Window
{
    private readonly Database _database;

    private MeetingAct? _previewMeetingAct;

    private string? _previewPdfPath;

    public MainWindow(Database database)
    {
        InitializeComponent();

        _database = database;

        ActForm.Configure(database);
    }

    private void NewActButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        ShowForm();

        ActForm.NewAct();
    }

    private async void HistoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        HistoryWindow historyWindow =
            new(_database)
            {
                Owner = this
            };

        bool? result =
            historyWindow.ShowDialog();

        if (result != true ||
            historyWindow.SelectedMeetingActId is not long id)
        {
            return;
        }

        await ActForm.LoadForReuseAsync(id);
    }

    private void ExportButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (_previewMeetingAct is null)
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Title = "Exportar acta",

            Filter =
                "Documento PDF (*.pdf)|*.pdf|" +
                "Documento Word (*.docx)|*.docx",

            FilterIndex = 1,

            AddExtension = true,

            DefaultExt = ".pdf",

            FileName =
                $"ACTA_{_previewMeetingAct.Header.DateTime:dd-MM-yyyy}"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            string extension =
                Path.GetExtension(
                    dialog.FileName
                ).ToLowerInvariant();

            switch (extension)
            {
                case ".pdf":
                    ExportPdf(
                        dialog.FileName
                    );
                    break;

                case ".docx":
                    ExportDocx(
                        dialog.FileName
                    );
                    break;

                default:
                    throw new NotSupportedException(
                        "El formato seleccionado no está soportado."
                    );
            }

            MessageBox.Show(
                "El acta fue exportada correctamente.",
                "ACTA",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"No fue posible exportar el acta.\n\n{exception.Message}",
                "ACTA",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    public void ShowPreview(string pdfPath, MeetingAct meetingAct)
    {
        _previewPdfPath = pdfPath;
        _previewMeetingAct = meetingAct;

        DocumentPreview.LoadDocument(pdfPath);

        ActForm.Visibility =
            Visibility.Collapsed;

        DocumentPreview.Visibility =
            Visibility.Visible;

        NewActButton.Visibility =
            Visibility.Collapsed;

        ExportButton.Visibility =
            Visibility.Visible;

        BackToFormButton.Visibility =
            Visibility.Visible;
    }

    private void ShowForm()
    {
        DocumentPreview.CloseDocument();

        DocumentPreview.Visibility =
            Visibility.Collapsed;

        ActForm.Visibility =
            Visibility.Visible;

        NewActButton.Visibility =
            Visibility.Visible;

        ExportButton.Visibility =
            Visibility.Collapsed;

        BackToFormButton.Visibility =
            Visibility.Collapsed;

        _previewMeetingAct = null;
        _previewPdfPath = null;
    }

    private void ExportPdf(
    string destinationPath)
    {
        if (_previewPdfPath is null ||
            !File.Exists(_previewPdfPath))
        {
            throw new FileNotFoundException(
                "No se encontró el PDF de la vista previa."
            );
        }

        File.Copy(
            _previewPdfPath,
            destinationPath,
            overwrite: true
        );
    }

    private void ExportDocx(
    string destinationPath)
    {
        if (_previewMeetingAct is null)
        {
            return;
        }

        IDocumentService documentService =
            DocumentServiceResolver.Get(
                _previewMeetingAct.GeneratorVersion
            );

        documentService.Create(
            destinationPath,
            _previewMeetingAct
        );
    }

    private void BackToFormButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowForm();
    }
}