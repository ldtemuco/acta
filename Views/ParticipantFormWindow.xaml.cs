using ACTA.Models;
using ACTA.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ACTA.Views;

public partial class ParticipantFormWindow : Window
{
    private static readonly Brush ValidBrush =
        new SolidColorBrush(Color.FromRgb(46, 125, 50));

    private static readonly Brush InvalidBrush =
        new SolidColorBrush(Color.FromRgb(198, 40, 40));

    private static readonly Brush DefaultBrush =
        new SolidColorBrush(Color.FromRgb(171, 173, 179));

    public Participant? Participant { get; private set; }

    private bool _isFormattingRun;

    public ParticipantFormWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => NameTextBox.Focus();
    }

    
    private void RunTextBox_PreviewTextInput(
    object sender,
    TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(character =>
            !char.IsDigit(character) &&
            character is not 'K' and not 'k'
        );
    }

    private void RunTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_isFormattingRun)
            return;

        _isFormattingRun = true;

        string formattedRun = FormatRun(RunTextBox.Text);

        RunTextBox.Text = formattedRun;
        RunTextBox.CaretIndex = formattedRun.Length;

        _isFormattingRun = false;

        ValidateFields();
    }

    private void RunTextBox_LostFocus(
        object sender,
        RoutedEventArgs e)
    {
        ValidateFields();
    }

    private static string FormatRun(string value)
    {
        string cleanRun = new(
            value
                .ToUpperInvariant()
                .Where(character =>
                    char.IsDigit(character) ||
                    character == 'K')
                .ToArray()
        );

        if (cleanRun.Length == 0)
            return string.Empty;

        // K solo puede ser dígito verificador.
        int kIndex = cleanRun.IndexOf('K');

        if (kIndex >= 0 && kIndex != cleanRun.Length - 1)
            cleanRun = cleanRun.Replace("K", "");

        // Máximo:
        // 9 dígitos de cuerpo + 1 DV.
        if (cleanRun.Length > 10)
            cleanRun = cleanRun[..10];

        string number;
        string? verifier = null;

        if (cleanRun.EndsWith('K'))
        {
            number = cleanRun[..^1];
            verifier = "K";
        }
        else if (cleanRun.Length >= 9)
        {
            number = cleanRun[..^1];
            verifier = cleanRun[^1].ToString();
        }
        else
        {
            number = cleanRun;
        }

        string formattedNumber = FormatRunNumber(number);

        return verifier is null
            ? formattedNumber
            : $"{formattedNumber}-{verifier}";
    }

    private static string FormatRunNumber(string number)
    {
        StringBuilder result = new();

        int count = 0;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % 3 == 0)
                result.Insert(0, '.');

            result.Insert(0, number[i]);

            count++;
        }

        return result.ToString();
    }

    private void Field_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateFields();
    }

    private void ValidateFields()
    {
        bool nameValid =
            !string.IsNullOrWhiteSpace(NameTextBox.Text);

        bool roleValid =
            !string.IsNullOrWhiteSpace(RoleTextBox.Text);

        bool runValid =
            ValidationService.IsValidRun(RunTextBox.Text);

        bool phoneValid =
            ValidationService.IsValidPhone(PhoneTextBox.Text);

        SetValidationState(
            NameTextBox,
            nameValid,
            NameTextBox.Text
        );

        SetValidationState(
            RoleTextBox,
            roleValid,
            RoleTextBox.Text
        );

        SetValidationState(
            RunTextBox,
            runValid,
            RunTextBox.Text
        );

        SetValidationState(
            PhoneTextBox,
            phoneValid,
            PhoneTextBox.Text
        );

        AddButton.IsEnabled =
            nameValid &&
            roleValid &&
            runValid &&
            phoneValid;
    }

    private static void SetValidationState(
        TextBox textBox,
        bool isValid,
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            textBox.ClearValue(BorderBrushProperty);
            return;
        }

        textBox.BorderBrush =
            isValid
                ? ValidBrush
                : InvalidBrush;
    }

    private void AddButton_Click(
        object sender,
        RoutedEventArgs e)
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
        return
            !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
            !string.IsNullOrWhiteSpace(RoleTextBox.Text) &&
            ValidationService.IsValidRun(RunTextBox.Text) &&
            ValidationService.IsValidPhone(PhoneTextBox.Text);
    }
}