namespace ACTA.Document;

using System.Drawing.Printing;
using PaperSize = Documents.DocumentPaperSize;

public static class DocumentLayout
{
    private const double TwipsPerInch = 1440.0;
    private const double CentimetersPerInch = 2.54;
    private const long EmusPerInch = 914400;

    // Página Oficio Chile: 21,59 x 33,02 cm
    public static readonly int PageWidth = CmToTwips(21.59);
    public static readonly int PageHeight = CmToTwips(33.02);

    // Márgenes: 1 cm
    public static readonly int MarginTop = CmToTwips(1.0);
    public static readonly int MarginBottom = CmToTwips(1.0);
    public static readonly int MarginLeft = CmToTwips(1.0);
    public static readonly int MarginRight = CmToTwips(1.0);

    // Ancho útil: 19,59 cm
    public static readonly int ContentWidth = CmToTwips(19.59);

    // Padding interno de celdas: 0,1 cm
    public static readonly int CellPadding = CmToTwips(0.1);

    // Columnas del encabezado
    public static readonly int HeaderLeftWidth = ContentWidth / 3;

    public static readonly int HeaderCenterWidth = ContentWidth / 3;

    public static readonly int HeaderRightWidth = ContentWidth - HeaderLeftWidth - HeaderCenterWidth;

    // Logo: 1,54 x 2,00 cm
    public static readonly long LogoWidth = CmToEmu(1.54);

    public static readonly long LogoHeight = CmToEmu(2.00);

    // Columnas de la tabla de participantes
    public static readonly int ParticipantNameWidth = CmToTwips(5.99);
    public static readonly int ParticipantRoleWidth = CmToTwips(4.75);
    public static readonly int ParticipantRunWidth = CmToTwips(2.50);
    public static readonly int ParticipantPhoneWidth = CmToTwips(2.75);
    public static readonly int ParticipantSignatureWidth = CmToTwips(3.60);

    // Altura de las filas de participantes: 1 cm
    public static readonly int ParticipantRowHeight = CmToTwips(1.0);

    public static int CmToTwips(double centimeters)
    {
        return (int)Math.Round(centimeters / CentimetersPerInch * TwipsPerInch);
    }

    public static int MmToTwips(double millimeters)
    {
        return CmToTwips(millimeters / 10.0);
    }

    public static long CmToEmu(double centimeters)
    {
        return (long)Math.Round(centimeters / CentimetersPerInch * EmusPerInch);
    }
}