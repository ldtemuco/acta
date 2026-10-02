namespace ACTA.Models;

public class MeetingAct
{
    public Header Header { get; set; } = new();

    public List<Participant> Participants { get; set; } = [];

    public string Motives { get; set; } = string.Empty;

    public string Agreements { get; set; } = string.Empty;

    public string Commitments { get; set; } = string.Empty;

    public int GeneratorVersion { get; set; }
}