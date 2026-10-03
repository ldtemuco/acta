using ACTA.Models;
using ACTA.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;

namespace ACTA.Views;

public partial class ParticipantFormWindow : Window
{
    // Indica si se está ejecutando la lógica de formateo del RUN para evitar bucles infinitos.
    private bool _isFormattingRun;

    // Color del borde de los TextBox cuando el valor es válido.
    private static readonly Brush ValidBrush = new SolidColorBrush(Color.FromRgb(46, 125, 50));

    // Color del borde de los TextBox cuando el valor es inválido.
    private static readonly Brush InvalidBrush = new SolidColorBrush(Color.FromRgb(198, 40, 40));

    public Participant? Participant
    {
        get;
        private set;
    }
    public ParticipantFormWindow()
    {
        InitializeComponent();
        DataObject.AddPastingHandler(RunTextBox, RunTextBox_Pasting);
        DataObject.AddPastingHandler(PhoneTextBox, PhoneTextBox_Pasting);
        DataObject.AddPastingHandler(NameTextBox, TextField_Pasting);
        DataObject.AddPastingHandler(RoleTextBox, TextField_Pasting);
        Loaded += (_, _) => NameTextBox.Focus();
    }

    public ParticipantFormWindow(Participant participant) : this()
    {
        Title = "Editar participante";
        AddButton.Content = "Guardar";
        NameTextBox.Text = participant.Name;
        RoleTextBox.Text = participant.Role;
        RunTextBox.Text = participant.Run;
        PhoneTextBox.Text = participant.Phone;
    }

    //=================================================================================================================
    // VALIDACIÓN DE NOMBRE Y ROL
    //=================================================================================================================


    private static bool IsValidTextCharacter(char character)
    {
        const string specialCharacters =
            "áéíóúÁÉÍÓÚäëïöüÄËÏÖÜñÑ";

        return
            (character >= 'A' && character <= 'Z') ||
            (character >= 'a' && character <= 'z') ||
            character == ' ' ||
            specialCharacters.Contains(character);
    }

    private void TextField_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(character => !IsValidTextCharacter(character));
    }

    private static void TextField_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.Text))
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

        bool isValid = text.All(IsValidTextCharacter);

        if (!isValid)
        {
            e.CancelCommand();
        }
    }

    private void TextField_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        { 
            return;
        }
        textBox.Text = NormalizeText(textBox.Text);
    }

    private static string NormalizeText(string text)
    {
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string normalizedText = string.Join(" ", words);

        TextInfo textInfo = CultureInfo.GetCultureInfo("es-CL").TextInfo;

        return textInfo.ToTitleCase(normalizedText.ToLower());
    }

    //=================================================================================================================
    // VALIDACIÓN DE TELÉFONO
    //=================================================================================================================

    private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(character => !char.IsDigit(character));
    }

    private void PhoneTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    private static void PhoneTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        string text = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;

        if (!text.All(char.IsDigit))
        {
            e.CancelCommand();
        }
    }

    //=================================================================================================================
    // VALIDACIÓN DEL RUN
    //=================================================================================================================

    private void RunTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = e.Text.Any(character =>
            !char.IsDigit(character) &&
            character is not 'K' and not 'k'
        );
    }

    private void RunTextBox_TextChanged(object sender, TextChangedEventArgs e)
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

    private static void RunTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        string text = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;

        bool valid = text.All(character => char.IsDigit(character) || character is 'K' or 'k' or '.' or '-'
        );

        if (!valid)
            e.CancelCommand();
    }


    private void RunTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ValidateFields();
    }

    private static string FormatRun(string value)
    {
        string upperRun = value.ToUpperInvariant();

        char[] validCharacters = upperRun.Where(character => char.IsDigit(character) || character == 'K').ToArray();

        string cleanRun = new(validCharacters);

        if (cleanRun.Length == 0)
        {
            return string.Empty;
        }
        // K solo puede ser dígito verificador.
        int kIndex = cleanRun.IndexOf('K');

        if (kIndex >= 0 && kIndex != cleanRun.Length - 1)
        {
            cleanRun = cleanRun.Replace("K", "");
        }
        // Máximo:
        // 9 dígitos de cuerpo + 1 DV.
        if (cleanRun.Length > 10)
        {
            cleanRun = cleanRun[..10];
        }
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
        bool nameValid = !string.IsNullOrWhiteSpace(NameTextBox.Text);

        bool roleValid = !string.IsNullOrWhiteSpace(RoleTextBox.Text);

        bool runValid = ValidationService.IsValidRun(RunTextBox.Text);

        bool phoneValid = ValidationService.IsValidPhone(PhoneTextBox.Text);

        SetValidationState(NameTextBox, nameValid, NameTextBox.Text);

        SetValidationState(RoleTextBox, roleValid, RoleTextBox.Text);

        SetValidationState(RunTextBox, runValid, RunTextBox.Text);

        SetValidationState(PhoneTextBox, phoneValid, PhoneTextBox.Text);

        AddButton.IsEnabled = nameValid && roleValid && runValid && phoneValid;
    }

    private static void SetValidationState(TextBox textBox, bool isValid, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            textBox.ClearValue(BorderBrushProperty);
            return;
        }

        textBox.BorderBrush = isValid ? ValidBrush : InvalidBrush;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm())
        {
            return;
        }

        Participant = new Participant
        {
            Name = NormalizeText(NameTextBox.Text),
            Role = NormalizeText(RoleTextBox.Text),
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