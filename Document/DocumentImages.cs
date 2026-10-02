namespace ACTA.Document;

public static class DocumentImages
{
    private const string BasePath = "pack://application:,,,/ACTA;component/Resources/Images/";

    public static readonly Uri Logo = new($"{BasePath}Logo.png", UriKind.Absolute);
}