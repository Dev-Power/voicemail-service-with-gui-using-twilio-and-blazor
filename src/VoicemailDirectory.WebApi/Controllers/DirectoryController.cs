using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using VoicemailDirectory.WebApi.Services;

namespace VoicemailDirectory.WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DirectoryController : TwilioController
{
    private readonly ILogger<DirectoryController> _logger;
    private readonly FileService _fileService;

    public DirectoryController(
        ILogger<DirectoryController> logger,
        FileService fileService
    )
    {
        _logger = logger;
        _fileService = fileService;
    }

    [HttpPost]
    public TwiMLResult Index()
    {
        var newMessages = _fileService.GetRecordingSids(Constants.New);
        var savedMessages = _fileService.GetRecordingSids(Constants.Saved);

        var response = new VoiceResponse();

        string GetWordingSingularOrPlural(int messageCount) => messageCount == 1 ? "message" : "messages";
        string GetNumberOrNo(int messageCount) => messageCount == 0 ? "no" : messageCount.ToString();
        response.Say(
            $"Hello, you have {GetNumberOrNo(newMessages.Count)} new {GetWordingSingularOrPlural(newMessages.Count)} " +
            $"and {GetNumberOrNo(savedMessages.Count)} saved {GetWordingSingularOrPlural(savedMessages.Count)}. "
        );

        // If there are no new or saved messages, end the call
        if (newMessages.Count == 0 && savedMessages.Count == 0)
        {
            response.Say("Goodbye!");
            return TwiML(response);
        }

        // Start with the new messages if there are any
        string recordingType = newMessages.Count > 0 ? Constants.New : Constants.Saved;
        response.Say($"Playing {recordingType} messages.");

        // No filter to get all recordings. Order alphabetically so that the new ones come at top
        // Can prepend datetime as well to order more precisely
        var allMessages = _fileService.GetRecordingSids(string.Empty)
            .OrderBy(s => s)
            .ToList();

        response.Append(
            CreateGatherTwiml(allMessages)
                .Append(PlayNextMessage(allMessages))
                .Append(SayOptions())
        );

        return TwiML(response);
    }

    [HttpPost]
    public TwiMLResult Gather(
        [FromQuery] List<string> queuedMessages,
        [FromForm] int digits
    )
    {
        _logger.LogInformation(
            "QueuedMessages: {queuedMessages}, user entered: {digits}",
            queuedMessages, digits
        );

        var currentMessage = queuedMessages.First();
        var isCurrentMessageNew = currentMessage.StartsWith(Constants.New);

        var response = new VoiceResponse();

        switch (digits)
        {
            case 1: // Replay
                // No action. The existing message will stay at the top of the queue to be replayed
                break;

            case 2: // Save
                _fileService.SaveRecording(currentMessage);
                queuedMessages.Remove(currentMessage);
                break;

            case 3: // Delete
                _fileService.DeleteRecording(currentMessage);
                queuedMessages.Remove(currentMessage);
                break;

            default: // Invalid key. Play an error message an repeat the valid options.
                response.Say("Sorry, that key is not valid.");
                response.Append(
                    CreateGatherTwiml(queuedMessages)
                        .Append(SayOptions())
                );
                return TwiML(response);
        }

        if (queuedMessages.Count == 0)
        {
            response.Say("No more messages. Goodbye!");
            return TwiML(response);
        }

        if (isCurrentMessageNew && queuedMessages.First().StartsWith(Constants.Saved))
        {
            response.Say("No more new messages. Here are your saved messages.");
        }

        response.Append(
            CreateGatherTwiml(queuedMessages)
                .Append(PlayNextMessage(queuedMessages))
                .Append(SayOptions())
        );

        return TwiML(response);
    }

    private Gather CreateGatherTwiml(List<string> queuedMessages) => new Gather(
        input: new List<Gather.InputEnum> {Twilio.TwiML.Voice.Gather.InputEnum.Dtmf},
        timeout: 5,
        numDigits: 1,
        action: new Uri(
            Url.Action("Gather", new {queuedMessages})!,
            UriKind.Relative
        ),
        method: Twilio.Http.HttpMethod.Post
    );

    private Say SayOptions()
        => new Say("To replay press 1. To save the message press 2. To delete the message press 3.");

    private Play PlayNextMessage(List<string> queuedMessages)
    {
        var nextMessage = queuedMessages.First();
        return new Play(new Uri($"/Voicemails/{nextMessage}.mp3", UriKind.Relative));
    }
}