namespace ACTA.Utilities;

public static class RunFormatter
{
    public static string Normalize(string? run)
    {
        if (string.IsNullOrWhiteSpace(run))
        {
            return string.Empty;
        }

        return run
            .Trim()
            .Replace(".", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();
    }

    public static string Format(string? run)
    {
        string normalizedRun = Normalize(run);

        if (normalizedRun.Length < 2)
        {
            return normalizedRun;
        }

        string number = normalizedRun[..^1];
        char verificationDigit = normalizedRun[^1];

        List<char> formattedNumber = [];

        int digitCount = 0;

        for (int index = number.Length - 1; index >= 0; index--)
        {
            if (digitCount > 0 && digitCount % 3 == 0)
            {
                formattedNumber.Add('.');
            }

            formattedNumber.Add(number[index]);

            digitCount++;
        }

        formattedNumber.Reverse();

        return $"{new string([.. formattedNumber])}-{verificationDigit}";
    }
}