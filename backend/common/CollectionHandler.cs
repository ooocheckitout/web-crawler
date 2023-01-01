using System.Text.Json;

public class CollectionHandler
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
