namespace common;

public class WebDownloader
{
    readonly HttpClient _client;

    public WebDownloader(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Downloading text from {url}");

        using var response = await _client.GetAsync(url, cancellationToken);
        using var content = response.Content;
        string stringContent = await content.ReadAsStringAsync(cancellationToken);

        return stringContent;
    }
}