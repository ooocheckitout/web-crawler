using System.Text.Json;

public class CollectionManager
{
    readonly FileReader _fileReader;
    readonly WebDownloader _downloader;
    readonly Hasher _hasher;
    readonly FileWriter _fileWriter;

    public CollectionManager(FileReader fileReader, WebDownloader downloader, Hasher hasher, FileWriter fileWriter)
    {
        _fileReader = fileReader;
        _downloader = downloader;
        _hasher = hasher;
        _fileWriter = fileWriter;
    }

    public Task<IEnumerable<Schema>> GetSchemaAsync(string collection)
    {
        return _fileReader.ReadJsonFileAsync<IEnumerable<Schema>>($"collections/{collection}/schema.json");
    }

    public Task<IEnumerable<string>> GetUrlsAsync(string collection)
    {
        return _fileReader.ReadJsonFileAsync<IEnumerable<string>>($"collections/{collection}/urls.json");
    }

    public Task<string> GetOrCreateHtmlContentAsync(string collection, string url)
    {
        var hash = _hasher.GetSha256HashAsHex(url);
        var fileLocation = $"collections/{collection}/content/{hash}.html";

        return File.Exists(fileLocation)
            ? _fileReader.ReadTextFileAsync(fileLocation)
            : _downloader.DownloadTextToFileAsync(url, fileLocation);
    }

    public Task CreateDataAsync(string collection, string schemaName, string url, IEnumerable<IDictionary<string, object>> objects)
    {
        var hash = _hasher.GetSha256HashAsHex(url);
        var fileLocation = $"collections/{collection}/data/{schemaName}/{hash}.json";

#if !DEBUG
        if (File.Exists(fileLocation))
            return Task.CompletedTask;
#endif

        return _fileWriter.ToJsonFileAsync(fileLocation, objects);
    }
}
