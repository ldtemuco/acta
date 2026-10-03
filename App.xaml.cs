using ACTA.Data;
using ACTA.Data.Repositories;
using ACTA.Models;
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

            await TestDatabaseConnection();

       
        }

        public async Task TestDatabaseConnection()
        {
            Database database = new();

            MeetingActRepository repository = new(database);

            MeetingAct original = new()
            {
                Header = new Header
                {
                    
                    DateTime = new DateTime(
                        2026,
                        10,
                        3,
                        16,
                        45,
                        0
                    )
                },

                Motives = "Reunión de prueba.",
                Agreements = "Acuerdo de prueba.",
                Commitments = "Compromiso de prueba.",

                GeneratorVersion = 1,

                Participants =
                [
                    new Participant
                    {
                        Name = "Anacleto Murfacio Coronado Prudencio",
                        Role = "Coordinadora de convivencia escolar.",
                        Run = "18.456.789-K",
                        Phone = "65465123"
                    },

                    new Participant
                    {
                        Name = "María Pérez",
                        Role = "Apoderada",
                        Run = "12.345.678-5",
                        Phone = "87654321"
                    }
                ]
            };

            long id = await repository.InsertAsync(original);

            MeetingAct? loaded = await repository.GetByIdAsync(id);

            if (loaded is null)
            {
                MessageBox.Show("No fue posible recuperar el acta.");
                return;
            }

            MessageBox.Show(
                $"""
                ID: {id}

                Ciudad: {loaded.Header.City}
                Fecha: {loaded.Header.DateTime:dd/MM/yyyy}
                Hora: {loaded.Header.DateTime:HH:mm}

                Participantes: {loaded.Participants.Count}

                Primero:
                {loaded.Participants[0].Name}
                {loaded.Participants[0].Run}

                Segundo:
                {loaded.Participants[1].Name}
                {loaded.Participants[1].Run}
                """
            );


        }
    }

   

}
