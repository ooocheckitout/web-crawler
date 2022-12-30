public class WebDownloader
{
    readonly HttpClient _client;
    readonly FileWriter _fileWriter;

    public WebDownloader(HttpClient client, FileWriter fileWriter)
    {
        _client = client;
        _fileWriter = fileWriter;
    }

    public async Task<string> DownloadTextToFileAsync(string url, string fileLocation)
    {
        using var response = await _client.GetAsync(url);
        using var content = response.Content;
        string stringContent = await content.ReadAsStringAsync();

        await _fileWriter.ToTextFileAsync(fileLocation, stringContent);

        return stringContent;
    }
}
