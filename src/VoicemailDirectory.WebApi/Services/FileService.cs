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
            $"{_rootVoicemailPath}/{Constants.New}_{recordingSid}.mp3",
            FileMode.CreateNew
        );
        await response.Content.CopyToAsync(fs);
    }

    public List<string> GetRecordingSids(string recordingType)
        => Directory.GetFiles($"{_rootVoicemailPath}/", $"{recordingType}*.mp3")
            .Select(s => Path.GetFileNameWithoutExtension(s))
            .ToList();

    public void SaveRecording(string recordingSid)
    {
        var currentPath = GetRecordingPathBySid(recordingSid);
        var newPath = currentPath.Replace($"{Constants.New}", $"{Constants.Saved}");
        File.Move(currentPath, newPath);
    }

    public void DeleteRecording(string recordingSid) => File.Delete(GetRecordingPathBySid(recordingSid));

    private string GetRecordingPathBySid(string recordingSid)
        => Directory.GetFiles($"{_rootVoicemailPath}/", "*.mp3")
            .Single(s => s.Contains(recordingSid));
}