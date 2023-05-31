namespace VoicemailDirectory.WebApi.Services;

public class FileService
{
    private readonly HttpClient _httpClient;
    private readonly string _rootVoicemailPath;

    public FileService(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
    {
        _rootVoicemailPath = $"{webHostEnvironment.WebRootPath}/Voicemails";
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task DownloadRecording(string recordingUrl, string recordingSid)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync($"{recordingUrl}.mp3");
        response.EnsureSuccessStatusCode();
        await using var fs = new FileStream(
            $"{_rootVoicemailPath}/{recordingSid}.mp3",
            FileMode.CreateNew
        );
        await response.Content.CopyToAsync(fs);
    }

    public void DeleteRecording(string recordingSid) => File.Delete(GetRecordingPathBySid(recordingSid));

    private string GetRecordingPathBySid(string recordingSid)
        => Directory.GetFiles($"{_rootVoicemailPath}/", "*.mp3")
            .Single(s => s.Contains(recordingSid));
}