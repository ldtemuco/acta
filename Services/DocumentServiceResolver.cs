using ACTA.Services;

public static class DocumentServiceResolver
{
    public static IDocumentService Get(int version)
    {
        return new DocumentServiceV1();
        /*
        return version switch
        {
            1 => new DocumentServiceV1(),

            _ => throw new NotSupportedException(
                $"La versión {version} del generador de documentos no está soportada para esta versión de ACTA.")
        };
        */
    }
}