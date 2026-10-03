using ACTA.Models;
using ACTA.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ACTA.Views;

public partial class ActFormView : UserControl
{
    private const int MaxParticipants = 10;

    private static readonly Brush ValidBrush = new SolidColorBrush(Color.FromRgb(46, 125, 50));

    private static readonly Brush InvalidBrush = new SolidColorBrush(Color.FromRgb(198, 40, 40));

    public ObservableCollection<Participant> Participants { get; } = [];
    public ActFormView()
    {
        InitializeComponent();

        MeetingDatePicker.SelectedDate = DateTime.Today;

        DataObject.AddPastingHandler(MotivesTextBox, ActTextBox_Pasting);

        DataObject.AddPastingHandler(AgreementsTextBox, ActTextBox_Pasting);

        DataObject.AddPastingHandler(CommitmentsTextBox, ActTextBox_Pasting);

        DataObject.AddPastingHandler(MeetingHourTextBox, TimeTextBox_Pasting);

        DataObject.AddPastingHandler(MeetingMinuteTextBox,TimeTextBox_Pasting);
    }

    private bool TryGetMeetingDateTime(out DateTime dateTime)
    {
        dateTime = default;

        if (MeetingDatePicker.SelectedDate is not DateTime date)
        { 
            return false; 
        }

        if (!int.TryParse(MeetingHourTextBox.Text, out int hour))
        {
            return false;
        }

        if (!int.TryParse(MeetingMinuteTextBox.Text, out int minute))
        {
            return false;
        }

        if (hour < 0 || hour > 23)
        { 
            return false; 
        }

        if (minute < 0 || minute > 59)
        { 
            return false; 
        }

        dateTime = date.Date.AddHours(hour).AddMinutes(minute);

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
            UpdateParticipantControls();
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

    private void EditParticipantButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.DataContext is not Participant participant)
        {
            return;
        }

        ParticipantFormWindow dialog = new(participant)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true ||
            dialog.Participant is null)
        {
            return;
        }

        int index = Participants.IndexOf(participant);

        if (index >= 0)
            Participants[index] = dialog.Participant;
    }

    private static void ShowValidationError(string message, Control? control = null)
    {
        MessageBox.Show(message, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
        control?.Focus();
    }

    private static bool IsValidActTextCharacter(char character)
    {
        const string specialCharacters = ".,¿?¡!ñÑáéíóúÁÉÍÓÚäëïöüÄËÏÖÜ-@";

        return
            (character >= 'A' && character <= 'Z') ||
            (character >= 'a' && character <= 'z') ||
            (character >= '0' && character <= '9') ||
            character == ' ' ||
            character == '\r' ||
            character == '\n' ||
            specialCharacters.Contains(character);
    }

    private void ActTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(character => !IsValidActTextCharacter(character));
    }

    private void ActTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        bool isValid =
            !string.IsNullOrWhiteSpace(textBox.Text) &&
            textBox.Text.Length <= 750 &&
            textBox.Text.All(IsValidActTextCharacter);

        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            textBox.ClearValue(BorderBrushProperty);
            return;
        }

        textBox.BorderBrush = isValid ? ValidBrush : InvalidBrush;
    }

    private static void ActTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox || !e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        object data = e.DataObject.GetData(DataFormats.Text);

        if (data is not string text)
        {
            e.CancelCommand();
            return;
        }

        bool validCharacters = text.All(IsValidActTextCharacter);

        bool validLength = textBox.Text.Length - textBox.SelectionLength + text.Length <= 750;

        bool consecutiveSpaces = text.Contains("  ");

        if (!validCharacters || !validLength || consecutiveSpaces)
        {
            e.CancelCommand();
        }
    }

    private void ActTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox ||
            e.Key != Key.Space)
        {
            return;
        }

        int position = textBox.CaretIndex;

        bool previousIsSpace = position > 0 && textBox.Text[position - 1] == ' ';

        bool nextIsSpace = position < textBox.Text.Length && textBox.Text[position] == ' ';

        if (position == 0 || previousIsSpace || nextIsSpace)
        {
            e.Handled = true;
        }
    }

    private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(character => !char.IsDigit(character));
    }

    private static void TimeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        object data = e.DataObject.GetData(DataFormats.Text);

        if (data is not string text || text.Length > 2 || !text.All(char.IsDigit))
        {
            e.CancelCommand();
        }
    }

    private void MeetingHourTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (MeetingHourTextBox.Text.Length == 2)
        {
            MeetingMinuteTextBox.Focus();
            MeetingMinuteTextBox.SelectAll();
        }
    }

    private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        { 
            return;
        }
        if (!int.TryParse(textBox.Text, out int value))
        {  
            return;
        }
        textBox.Text = value.ToString("00");
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
            ShowValidationError("Ingrese una hora válida entre 00:00 y 23:59.", MeetingHourTextBox);
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

        if (Participants.Count > MaxParticipants)
        {
            ShowValidationError( $"El acta no puede contener más de {MaxParticipants} participantes." );

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