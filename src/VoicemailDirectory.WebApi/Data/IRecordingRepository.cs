namespace VoicemailDirectory.WebApi.Data;

public interface IRecordingRepository
{
    Task NewRecording(Recording recording);
    Task UpdateCallerAndTranscription(string recordingSID, string caller, string transcriptionText);
    Task ChangeStatusToSaved(string recordingSID);
    Task Delete(string recordingSID);
    List<Recording> GetAll();
}
