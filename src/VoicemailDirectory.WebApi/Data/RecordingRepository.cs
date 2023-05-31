using VoicemailDirectory.WebApi.Data;

public class RecordingRepository : IRecordingRepository
{
    private readonly RecordingContext _recordingContext;
    
    public RecordingRepository(RecordingContext recordingContext)
    {
        _recordingContext = recordingContext;
    }

    public async Task NewRecording(Recording recording)
    {
        await _recordingContext.Recordings.AddAsync(recording);
        await _recordingContext.SaveChangesAsync();
    }
    
    public async Task UpdateCallerAndTranscription(string recordingSID, string caller, string transcriptionText)
    {
        var recording = _recordingContext.Recordings.Single(rec => rec.RecordingSID == recordingSID);
        recording.CallerNumber = caller;
        recording.Transcription = transcriptionText;
        await _recordingContext.SaveChangesAsync();
    }
    
    public async Task ChangeStatusToSaved(string recordingSID)
    {
        var recording = _recordingContext.Recordings.Single(rec => rec.RecordingSID == recordingSID);
        recording.Status = RecordingStatus.Saved;
        await _recordingContext.SaveChangesAsync();
    }

    public async Task Delete(string recordingSID)
    {
        var recording = _recordingContext.Recordings.Single(rec => rec.RecordingSID == recordingSID);
        _recordingContext.Recordings.Remove(recording);
        await _recordingContext.SaveChangesAsync();
    }

    public List<Recording> GetAll()
    {
        return _recordingContext.Recordings.ToList();
    }
}