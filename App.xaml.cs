using ACTA.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ACTA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Database database = new();

            DatabaseInitializer databaseInitializer = new(database);

            await databaseInitializer.InitializeAsync();
        }
    }

   

}
