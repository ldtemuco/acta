using ACTA.Data;
using System.Windows;

namespace ACTA;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Database database = new();

            DatabaseInitializer initializer = new(database);

            await initializer.InitializeAsync();

            MainWindow mainWindow = new(database);

            MainWindow = mainWindow;

            mainWindow.Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"No fue posible iniciar ACTA.\n\n{exception.Message}",
                "ACTA",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            Shutdown(-1);
        }
    }
}