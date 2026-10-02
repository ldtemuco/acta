using ACTA.Models;

namespace ACTA.Services;

public interface IDocumentService
{
    void Create(string filePath, MeetingAct meetingAct);
}