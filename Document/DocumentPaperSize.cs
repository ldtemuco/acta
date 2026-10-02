namespace ACTA.Documents;

public static class DocumentPaperSize
{
    private const double Dpi = 96.0;
    private const double CentimetersPerInch = 2.54;
    public static double PaperWidth => 21.59 / CentimetersPerInch * Dpi;
    public static double PaperHeight => 33.02 / CentimetersPerInch * Dpi;
}