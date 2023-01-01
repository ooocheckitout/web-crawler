using System.Net;
using System.Text.Json;
using Xunit;

namespace crawler.tests;

class CollectionLocator
{
    private readonly string _collectionsRoot;
    private readonly Hasher _hasher;

    public CollectionLocator(string collectionsRoot, Hasher hasher)
    {
        _collectionsRoot = collectionsRoot;
        _hasher = hasher;
    }

    public string GetRoot()
    {
        return _collectionsRoot;
    }

    public string GetSchemasLocation(string collection)
    {
        return $"{_collectionsRoot}/{collection}/schemas.json";
    }

    public string GetUrlsLocation(string collection)
    {
        return $"{_collectionsRoot}/{collection}/urls.json";
    }

    public string GetHtmlLocation(string collection, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/content/{hash}.html";
    }

    public string GetDataLocation(string collection, string schema, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/data/{schema}/{hash}.json";
    }

    public string GetChecksumLocation(string collection, string schema, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/data/{schema}/{hash}.checksum";
    }
}

class CollectionFactory
{
    private readonly CollectionLocator _locator;
    private readonly FileReader _fileReader;

    public CollectionFactory(CollectionLocator locator, FileReader fileReader)
    {
        _locator = locator;
        _fileReader = fileReader;
    }

    public async Task<Collection> GetSingleAsync(string collectionName)
    {
        return new Collection
        {
            Name = collectionName,
            Urls = await GetUrlsAsync(collectionName),
            Schemas = await GetSchemasAsync(collectionName)
        };
    }

    public Task<Collection[]> GetAllAsync()
    {
        var collections = Directory.EnumerateDirectories(_locator.GetRoot()).Select(x => Path.GetFileName(x)!);
        return Task.WhenAll(collections.Select(GetSingleAsync));
    }

    private Task<IEnumerable<string>> GetUrlsAsync(string collectionName)
    {
        string location = _locator.GetUrlsLocation(collectionName);
        return _fileReader.ReadJsonAsync<IEnumerable<string>>(location);
    }

    private Task<IEnumerable<Schema>> GetSchemasAsync(string collectionName)
    {
        string location = _locator.GetSchemasLocation(collectionName);
        return _fileReader.ReadJsonAsync<IEnumerable<Schema>>(location);
    }
}

class Collection
{
    public string Name { get; init; }
    public IEnumerable<string> Urls { get; init; } = Array.Empty<string>();
    public IEnumerable<Schema> Schemas { get; init; } = Array.Empty<Schema>();
}

class CollectionHandler
{
    private readonly CollectionLocator _locator;
    private readonly WebDownloader _downloader;
    private readonly FileReader _fileReader;
    private readonly Parser _parser;
    private readonly FileWriter _fileWriter;
    private readonly Hasher _hasher;

    public CollectionHandler(
        CollectionLocator locator,
        WebDownloader downloader,
        FileReader fileReader,
        Parser parser,
        FileWriter fileWriter,
        Hasher hasher)
    {
        _locator = locator;
        _downloader = downloader;
        _fileReader = fileReader;
        _parser = parser;
        _fileWriter = fileWriter;
        _hasher = hasher;
    }

    public async Task HandleAsync(Collection collection)
    {
        foreach (var schema in collection.Schemas)
        {
            foreach (string url in collection.Urls)
            {
                string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);
                if (!File.Exists(htmlLocation))
                    await _downloader.DownloadTextToFileAsync(url, htmlLocation);

                string htmlContent = await _fileReader.ReadTextAsync(htmlLocation);

                string checksum = CalculateChecksum(schema, htmlContent);
                string checksumLocation = _locator.GetChecksumLocation(collection.Name, schema.Name, url);
                string existingChecksum = File.Exists(checksumLocation) ? await _fileReader.ReadTextAsync(checksumLocation) : string.Empty;

                if (checksum == existingChecksum)
                    continue;

                var objects = _parser.Parse(htmlContent, schema);

                string dataLocation = _locator.GetDataLocation(collection.Name, schema.Name, url);
                await _fileWriter.AsJsonAsync(dataLocation, objects);
            }
        }
    }

    private string CalculateChecksum(Schema schema, string htmlContent)
    {
        string schemaChecksum = _hasher.GetSha256HashAsHex(JsonSerializer.Serialize(schema));
        string contentChecksum = _hasher.GetSha256HashAsHex(htmlContent);
        return _hasher.GetSha256HashAsHex(schemaChecksum + contentChecksum);
    }
}

public class Tests
{
    [Fact]
    public async Task CollectionFactory()
    {
        const string collectionsRoot = @"D:\code\web-crawler\collections";
        var hasher = new Hasher();
        var locator = new CollectionLocator(collectionsRoot, hasher);
        var fileReader = new FileReader();
        var factory = new CollectionFactory(locator, fileReader);
        var fileWriter = new FileWriter();
        var handler = new CollectionHandler(
            locator, new WebDownloader(new HttpClient(), fileWriter), fileReader, new Parser(), fileWriter, hasher);

        var collection = await factory.GetSingleAsync("minfin-petrol-areas");
        await handler.HandleAsync(collection);

        // foreach (var collection in await factory.GetAllAsync())
        // {
        //     await handler.HandleAsync(collection);
        // }
    }
}
