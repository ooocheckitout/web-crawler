using System.Text.Json;

namespace common;

public class CollectionHandler
{
    readonly CollectionLocator _locator;
    readonly WebDownloader _downloader;
    readonly FileReader _fileReader;
    readonly Parser _parser;
    readonly FileWriter _fileWriter;
    readonly Hasher _hasher;

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
        foreach (string url in collection.Urls)
        {
            string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);
            if (!File.Exists(htmlLocation))
                await _downloader.DownloadTextToFileAsync(url, htmlLocation);

            string htmlContent = await _fileReader.ReadTextAsync(htmlLocation);

            string checksum = CalculateChecksum(collection.Schema, htmlContent);
            string checksumLocation = _locator.GetChecksumLocation(collection.Name, url);
            string existingChecksum = File.Exists(checksumLocation)
                ? await _fileReader.ReadTextAsync(checksumLocation)
                : string.Empty;

            if (checksum == existingChecksum)
                continue;

            var objects = _parser.Parse(htmlContent, collection.Schema);

            string dataLocation = _locator.GetBronzeFileLocation(collection.Name, url);
            await _fileWriter.AsJsonAsync(dataLocation, objects);

            await _fileWriter.AsTextAsync(checksumLocation, checksum);
        }
    }

    string CalculateChecksum(Schema schema, string htmlContent)
    {
        string schemaChecksum = _hasher.GetSha256HashAsHex(JsonSerializer.Serialize(schema));
        string contentChecksum = _hasher.GetSha256HashAsHex(htmlContent);
        return _hasher.GetSha256HashAsHex(schemaChecksum + contentChecksum);
    }
}
