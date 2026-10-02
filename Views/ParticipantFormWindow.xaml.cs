using ACTA.Models;
using ACTA.Services;

using System.Windows;

namespace ACTA.Views;

public partial class ParticipantFormWindow : Window
{
    public Participant? Participant { get; private set; }

    public ParticipantFormWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => NameTextBox.Focus();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm())
            return;

        Participant = new Participant
        {
            Name = NameTextBox.Text.Trim(),
            Role = RoleTextBox.Text.Trim(),
            Run = RunTextBox.Text.Trim(),
            Phone = PhoneTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ShowValidationError(
                "Debe ingresar el nombre del participante.",
                NameTextBox
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(RoleTextBox.Text))
        {
            ShowValidationError(
                "Debe ingresar el rol del participante.",
                RoleTextBox
            );

            return false;
        }

        if (!ValidationService.IsValidRun(RunTextBox.Text))
        {
            ShowValidationError(
                "Debe ingresar un RUN válido.",
                RunTextBox
            );

            return false;
        }

        if (!ValidationService.IsValidPhone(PhoneTextBox.Text))
        {
            ShowValidationError(
                "Debe ingresar un teléfono válido.",
                PhoneTextBox
            );

            return false;
        }

        return true;
    }

    private static void ShowValidationError(
        string message,
        System.Windows.Controls.Control control)
    {
        MessageBox.Show(
            message,
            "Datos inválidos",
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );

        control.Focus();
    }
}