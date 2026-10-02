using ACTA.Document;

public class Header
{
    public string City => "Temuco";

    public DateTime DateTime { get; set; }

    public override string ToString()
    {
        return
            $"{City} – " +
            $"{DocumentHelpers.FormatDate(DateTime)} – " +
            $"{DocumentHelpers.FormatTime(DateTime)}";
    }
}