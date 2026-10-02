using ACTA.Models;
using ACTA.Services;
using System.IO;
using System.Windows;

namespace ACTA;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();



        //Loaded += MainWindow_Loaded;

        // GenerateTestDocument();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string filePath = Path.Combine(desktop,"ACTA_TEST.pdf");
        //DocumentPreview.LoadDocument(filePath);
    }

    private static void GenerateTestDocument()
    {
        MeetingAct meetingAct = new()
        {
            Header = new Header
            {                
                DateTime = new DateTime(2026, 9, 24, 16, 45, 0)
            },

            Motives = "Reunión para analizar la situación académica y de convivencia del estudiante.",
            Agreements = "Se acuerda realizar seguimiento durante las próximas semanas.",
            Commitments = "El establecimiento mantendrá comunicación con el apoderado."
        };

        meetingAct.Participants.Add(
            new Participant
            {
                Name = "Anacleto Murfacio Coronado",
                Role = "Apoderado",
                Run = "12.345.678-9",
                Phone = "65465123"
            }
        );

        meetingAct.Participants.Add(
            new Participant
            {
                Name = "Prudencia González",
                Role = "Coordinadora de convivencia escolar",
                Run = "11.222.333-4",
                Phone = "87654321"
            }
        );

        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        string filePath = Path.Combine(desktop, "ACTA_Prueba.docx");

        IDocumentService documentService = DocumentServiceResolver.Get(meetingAct.GeneratorVersion);

        documentService.Create(filePath, meetingAct);

        MessageBox.Show(
            $"Documento generado correctamente:\n\n{filePath}",
            "ACTA",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}