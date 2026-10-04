using ACTA.Data;
using ACTA.Views;
using System.Windows;

namespace ACTA;

public partial class MainWindow : Window
{
    private readonly Database _database;

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
}