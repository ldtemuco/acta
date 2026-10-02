namespace ACTA.Document;

public static class DocumentFonts
{
    private const string BasePath = "pack://application:,,,/ACTA;component/Resources/Fonts/Roboto/";

    public const string FamilyName = "Roboto";

    public static readonly Uri Regular = CreateUri("Roboto-Regular.ttf");

    public static readonly Uri Bold = CreateUri("Roboto-Bold.ttf");

    public static readonly Uri Italic = CreateUri("Roboto-Italic.ttf");

    public static readonly Uri BoldItalic = CreateUri("Roboto-BoldItalic.ttf");

    private static Uri CreateUri(string fileName)
    {
        return new Uri($"{BasePath}{fileName}", UriKind.Absolute);
    }
}