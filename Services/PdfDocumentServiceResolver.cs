namespace ACTA.Services;

public static class PdfDocumentServiceResolver
{
    public static IDocumentService Get(
        int version)
    {
        return version switch
        {
            1 => new PdfDocumentServiceV1(),

            _ => throw new NotSupportedException(
                $"La versión {version} del generador PDF " +
                "no está soportada."
            )
        };
    }
}