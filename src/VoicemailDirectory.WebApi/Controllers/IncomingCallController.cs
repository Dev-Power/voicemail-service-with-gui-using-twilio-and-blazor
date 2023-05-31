using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Twilio.AspNet.Core;
using Twilio.TwiML;


namespace VoicemailDirectory.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class IncomingCallController : TwilioController
{
    private readonly VoicemailOptions _voicemailOptions;


    public IncomingCallController(IOptionsSnapshot<VoicemailOptions> voicemailOptions)
    {
        _voicemailOptions = voicemailOptions.Value;
    }


    [HttpPost]
    public TwiMLResult Index([FromForm] string from)
    {
        var response = new VoiceResponse();
        
        var redirectUrl = _voicemailOptions.Owners.Contains(from)
            ? Url.Action("Index", "Directory")!
            : Url.Action("Index", "Record")!;


        response.Redirect(
            url: new Uri(redirectUrl, UriKind.Relative),
            method: Twilio.Http.HttpMethod.Post
        );
        
        return TwiML(response);
    }
}