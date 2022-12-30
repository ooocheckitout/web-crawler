using System.Text.Json;

public class CollectionManager
{
    private readonly string _collectionsRoot;
    private readonly FileReader _fileReader;
    private readonly Hasher _hasher;
    private readonly WebDownloader _downloader;
    private readonly FileWriter _fileWriter;

    public CollectionManager(
        string collectionsRoot,
        FileReader fileReader,
        Hasher hasher,
        WebDownloader downloader,
        FileWriter fileWriter)
    {
        _collectionsRoot = collectionsRoot;
        _fileReader = fileReader;
        _hasher = hasher;
        _downloader = downloader;
        _fileWriter = fileWriter;
    }

    public IEnumerable<string> GetCollections()
    {
        return Directory.EnumerateDirectories(_collectionsRoot).Select(Path.GetFileName)!;
    }

    public Task<IEnumerable<string>> GetUrlsAsync(string collection)
    {
        return _fileReader.ReadJsonFileAsync<IEnumerable<string>>($"{_collectionsRoot}/{collection}/urls.json");
    }

    public Task<IEnumerable<Schema>> GetSchemasAsync(string collection)
    {
        return _fileReader.ReadJsonFileAsync<IEnumerable<Schema>>($"{_collectionsRoot}/{collection}/schemas.json");
    }

    public Task<string> GetHtmlAsync(string collection, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        var fileLocation = $"{_collectionsRoot}/{collection}/content/{hash}.html";

        return File.Exists(fileLocation)
            ? _fileReader.ReadTextFileAsync(fileLocation)
            : _downloader.DownloadTextToFileAsync(url, fileLocation);
    }

    public Task SaveDataAsync(string collection, string schemaName, string url, IEnumerable<IDictionary<string, object>> objects)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        var fileLocation = $"{_collectionsRoot}/{collection}/data/{schemaName}/{hash}.json";

        return _fileWriter.ToJsonFileAsync(fileLocation, objects);
    }
}
