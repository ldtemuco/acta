using ACTA.Services;
using ACTA.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;       

namespace ACTA.Views;

public partial class ActFormView : UserControl
{
    private const int MaxParticipants = 10;

    public ObservableCollection<Participant> Participants { get; } = [];
    public ActFormView()
    {
        InitializeComponent();

        MeetingDatePicker.SelectedDate = DateTime.Today;
    }

    private bool TryGetMeetingDateTime(out DateTime dateTime)
    {
        dateTime = default;

        if (MeetingDatePicker.SelectedDate is not DateTime date)
            return false;

        if (!ValidationService.TryParseTime(MeetingTimeTextBox.Text, out TimeOnly time))
        {
            return false;
        }

        dateTime = date.Date.Add(time.ToTimeSpan());

        return true;
    }

    private void AddParticipantButton_Click(object sender, RoutedEventArgs e)
    {
        if (Participants.Count >= MaxParticipants)
        {
            MessageBox.Show(
                $"El acta permite un máximo de {MaxParticipants} participantes.",
                "Límite de participantes",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            return;
        }

        ParticipantFormWindow dialog = new()
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true &&
            dialog.Participant is not null)
        {
            Participants.Add(dialog.Participant);
        }
    }

    private void RemoveParticipantButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is Participant participant)
        {
            Participants.Remove(participant);

            UpdateParticipantControls();
        }
    }

    private void UpdateParticipantControls()
    {
        int count = Participants.Count;

        ParticipantCountTextBlock.Text = $"({count}/{MaxParticipants})";

        AddParticipantButton.IsEnabled = count < MaxParticipants;
    }


    private static void ShowValidationError(string message, Control? control = null)
    {
        MessageBox.Show(message, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
        control?.Focus();
    }
    private bool ValidateForm()
    {
        if (MeetingDatePicker.SelectedDate is null)
        {
            ShowValidationError("Debe escribir la fecha del acta.", MeetingDatePicker);

            return false;
        }

        if (!TryGetMeetingDateTime(out _))
        {
            ShowValidationError("Ingrese una hora válida en formato HH:mm.", MeetingTimeTextBox);
            return false;
        }

        if (Participants.Count < 2)
        {
            MessageBox.Show(
                "Debe agregar al menos dos participantes.",
                "Datos incompletos",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            return false;
        }

        foreach (Participant participant in Participants)
        {
            if (string.IsNullOrWhiteSpace(participant.Name))
            {
                ShowValidationError("Todos los participantes deben tener un nombre y apellido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(participant.Role))
            {
                ShowValidationError($"Debe ingresar el rol de {participant.Name}.");
                return false;
            }

            if (!ValidationService.IsValidRun(participant.Run))
            {
                ShowValidationError($"El RUN de {participant.Name} no es válido.");
                return false;
            }

            if (!ValidationService.IsValidPhone(participant.Phone))
            {
                ShowValidationError($"El teléfono de {participant.Name} no es válido.");
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(MotivesTextBox.Text))
        {
            ShowValidationError("Debe ingresar el motivo de la reunión.", MotivesTextBox);
            return false;
        }

        if (string.IsNullOrWhiteSpace(AgreementsTextBox.Text))
        {
            ShowValidationError("Debe ingresar los acuerdos de la reunión.", AgreementsTextBox);
            return false;
        }

        if (string.IsNullOrWhiteSpace(CommitmentsTextBox.Text))
        {
            ShowValidationError("Debe ingresar los compromisos de la reunión.", CommitmentsTextBox);
            return false;
        }

        return true;
    }
}