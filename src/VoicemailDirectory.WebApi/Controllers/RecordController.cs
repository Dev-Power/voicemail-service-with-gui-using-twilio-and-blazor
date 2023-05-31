using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using VoicemailDirectory.WebApi.Services;

namespace VoicemailDirectory.WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RecordController : TwilioController
{
    private readonly ILogger<IncomingCallController> _logger;
    private readonly FileService _fileService;

    public RecordController(
        ILogger<IncomingCallController> logger,
        FileService fileService
    )
    {
        _logger = logger;
        _fileService = fileService;
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
            recordingStatusCallbackMethod: Twilio.Http.HttpMethod.Post
        );
        return TwiML(response);
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
        [FromForm] string recordingStatus
    )
    {
        _logger.LogInformation(
            "Recording status changed to {recordingStatus} for call {callSid}. Recording is available at {recordingUrl}",
            recordingStatus, callSid, recordingUrl
        );

        if (recordingStatus == "completed")
        {
            await _fileService.DownloadRecording(recordingUrl, recordingSid);
        }
    }
}