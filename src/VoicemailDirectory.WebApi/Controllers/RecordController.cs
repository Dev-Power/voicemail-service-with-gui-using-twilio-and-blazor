using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using VoicemailDirectory.WebApi.Data;
using VoicemailDirectory.WebApi.Services;

namespace VoicemailDirectory.WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RecordController : TwilioController
{
    private readonly ILogger<IncomingCallController> _logger;
    private readonly FileService _fileService;
    private readonly IRecordingRepository _recordingRepository;
    
    public RecordController(
        ILogger<IncomingCallController> logger,
        FileService fileService,
        IRecordingRepository recordingRepository
    )
    {
        _logger = logger;
        _fileService = fileService;
        _recordingRepository = recordingRepository;
    }

    [HttpPost]
    public TwiMLResult Index()
    {
        var response = new VoiceResponse();
        response.Say("Hello, please leave a message after the beep.");
        response.Record(
            timeout: 10,
            action: new Uri(Url.Action("Bye")!, UriKind.Relative),
            method: Twilio.Http.HttpMethod.Post,
            recordingStatusCallback: new Uri(Url.Action("RecordingStatus")!, UriKind.Relative),
            recordingStatusCallbackMethod: Twilio.Http.HttpMethod.Post,
            transcribe: true,
            transcribeCallback: new Uri(Url.Action("TranscribeCallback")!, UriKind.Relative)
        );
        return TwiML(response);
    }

    [HttpPost]
    public async Task TranscribeCallback(
        [FromForm] string recordingSid,
        [FromForm] string transcriptionText,
        [FromForm] string from
    )
    {
        _logger.LogInformation(
            "Updating the recording {recordingSid} from {from} with transcription: {transcriptionText}",
            recordingSid, from, transcriptionText
        );
        
        await _recordingRepository.UpdateCallerAndTranscription(recordingSid, from, transcriptionText);
    }
    
    [HttpPost]
    public TwiMLResult Bye() => new VoiceResponse()
        .Say("Thank you for leaving a message, goodbye.")
        .ToTwiMLResult();

    [HttpPost]
    public async Task RecordingStatus(
        [FromForm] string callSid,
        [FromForm] string recordingUrl,
        [FromForm] string recordingSid,
        [FromForm] string recordingStatus,
        [FromForm] string recordingStartTime,
        [FromForm] string recordingDuration
    )
    {
        _logger.LogInformation(
            "Recording status changed to {recordingStatus} for call {callSid}. Recording is available at {recordingUrl}",
            recordingStatus, callSid, recordingUrl
        );

        if (recordingStatus == "completed")
        {
            await _recordingRepository.NewRecording(new Recording
            {
                RecordingSID = recordingSid,
                Status = Data.RecordingStatus.New,
                Date = DateTime.Parse(recordingStartTime),
                Duration = TimeSpan.FromSeconds(double.Parse(recordingDuration)),
                CallerNumber = string.Empty,
                Transcription = string.Empty
            });
            
            await _fileService.DownloadRecording(recordingUrl, recordingSid);
        }
    }
}