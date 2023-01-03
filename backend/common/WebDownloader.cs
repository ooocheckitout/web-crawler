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
        Console.WriteLine($"Downloading text from {url} to {fileLocation}");

        using var response = await _client.GetAsync(url);
        using var content = response.Content;
        string stringContent = await content.ReadAsStringAsync();

        await _fileWriter.AsTextAsync(fileLocation, stringContent);

        return stringContent;
    }
}
