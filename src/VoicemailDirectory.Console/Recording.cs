namespace VoicemailDirectory.Console;

public class Recording
{
    public int Id { get; set; }
    public string RecordingSID { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
    public string CallerNumber { get; set; }
    public string Transcription { get; set; }
    public RecordingStatus Status { get; set; }
}

public enum RecordingStatus
{
    New,
    Saved
}