using System.Text.RegularExpressions;
using System.Globalization;
namespace ACTA.Services;

public static partial class ValidationService
{
    public static bool IsValidRun(string? run)
    {
        if (string.IsNullOrWhiteSpace(run))
            return false;

        string cleanRun = run.Replace(".", "").Replace("-", "").Trim().ToUpperInvariant();

        if (cleanRun.Length < 2)
            return false;

        string numberPart = cleanRun[..^1];
        char verifier = cleanRun[^1];

        if (!int.TryParse(numberPart, out int number))
            return false;

        int sum = 0;
        int multiplier = 2;

        while (number > 0)
        {
            sum += number % 10 * multiplier;
            number /= 10;

            multiplier++;

            if (multiplier > 7)
                multiplier = 2;
        }

        int result = 11 - sum % 11;

        char expectedVerifier = result switch
        {
            11 => '0',
            10 => 'K',
            _ => result.ToString()[0]
        };

        return verifier == expectedVerifier;
    }

    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        return Regex.IsMatch(phone.Trim(), @"^\d{8}$");
    }

    public static bool TryParseTime(string? value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(
            value?.Trim(),
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time
        );
    }
}