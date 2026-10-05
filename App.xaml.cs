using ACTA.Data;
using ACTA.Views;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ACTA;

public partial class App : Application
{
    private const int MinimumSplashTimeMs = 4000;

    protected override async void OnStartup(StartupEventArgs e)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        base.OnStartup(e);

        SplashWindow? splash = null;

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            /*
             * Mostrar Splash
             */
            splash = new SplashWindow();

            splash.Show();

            splash.SetProgress(10, "Iniciando ACTA...");

            await Dispatcher.Yield(DispatcherPriority.Background);


            /*
             * Preparar base de datos
             */
            splash.SetProgress(30, "Preparando base de datos...");

            Database database = new();

            DatabaseInitializer initializer =
                new(database);

            await initializer.InitializeAsync();


            /*
             * Crear ventana principal
             */
            splash.SetProgress(75, "Cargando interfaz...");

            MainWindow mainWindow = new(database);

            MainWindow = mainWindow;


            /*
             * Carga terminada
             */
            splash.SetProgress(100, "Carga completada.");


            /*
             * Mantener el Splash visible
             * al menos 5 segundos.
             */
            stopwatch.Stop();

            int remainingTime = MinimumSplashTimeMs - (int)stopwatch.ElapsedMilliseconds;

            if (remainingTime > 0)
            {
                await Task.Delay(remainingTime);
            }


            /*
             * Mostrar ventana principal
             */
            mainWindow.Show();


            /*
             * Desvanecer Splash
             */
            await splash.FadeOutAsync();

            splash.Close();
        }
        catch (Exception exception)
        {
            splash?.Close();

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