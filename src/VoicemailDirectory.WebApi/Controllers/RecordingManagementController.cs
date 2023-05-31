using Microsoft.AspNetCore.Mvc;
using VoicemailDirectory.WebApi.Data;
using VoicemailDirectory.WebApi.Services;

namespace VoicemailDirectory.WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RecordingManagementController : ControllerBase
{
    private readonly FileService _fileService;
    private readonly IRecordingRepository _recordingRepository;
    
    public RecordingManagementController(IRecordingRepository recordingRepository, FileService fileService)
    {
        _recordingRepository = recordingRepository;
        _fileService = fileService;
    }

    [HttpGet]
    public ActionResult Index()
    {
        var recordings = _recordingRepository.GetAll();
        return Ok(recordings.OrderByDescending(m => m.Date));
    }
    
    [HttpPatch("{recordingSid}")]
    public async Task<ActionResult> Save(string recordingSid)
    {
        await _recordingRepository.ChangeStatusToSaved(recordingSid);
        return NoContent();
    }
    
    [HttpDelete("{recordingSid}")]
    public async Task<ActionResult> Delete(string recordingSid)
    {
        await _recordingRepository.Delete(recordingSid);
        _fileService.DeleteRecording(recordingSid);
        return NoContent();
    }
}