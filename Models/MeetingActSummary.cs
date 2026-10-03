namespace ACTA.Models;

public sealed class MeetingActSummary
{
    public long Id { get; init; }

    public DateTime DateTime { get; init; }

    public string Motives { get; init; } = string.Empty;

    public int ParticipantCount { get; init; }
}