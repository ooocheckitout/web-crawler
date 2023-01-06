using System.Text.Json;
using common.Bronze;
using common.Silver;

namespace common;

public class CollectionRunner
{
    readonly CollectionLocator _locator;
    readonly WebDownloader _downloader;
    readonly FileReader _fileReader;
    readonly Parser _parser;
    readonly FileWriter _fileWriter;
    readonly Hasher _hasher;
    readonly Transformer _transformer;

    public CollectionRunner(
        CollectionLocator locator,
        WebDownloader downloader,
        FileReader fileReader,
        Parser parser,
        FileWriter fileWriter,
        Hasher hasher,
        Transformer transformer)
    {
        _locator = locator;
        _downloader = downloader;
        _fileReader = fileReader;
        _parser = parser;
        _fileWriter = fileWriter;
        _hasher = hasher;
        _transformer = transformer;
    }

    public Task RunLoader(Collection collection, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        foreach (string url in collection.Urls)
        {
            string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);
            if (!File.Exists(htmlLocation))
                continue;

            tasks.Add(_downloader.DownloadTextToFileAsync(url, htmlLocation, cancellationToken));
        }

        return Task.WhenAll(tasks);
    }

    public async Task RunParser(Collection collection, CancellationToken cancellationToken)
    {
        foreach (string url in collection.Urls)
        {
            string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);
            string htmlContent = await _fileReader.ReadTextAsync(htmlLocation, cancellationToken);

            string checksum = CalculateChecksum(htmlContent, collection.TransformerSchema);
            string checksumLocation = _locator.GetChecksumLocation(collection.Name, url, Medallion.Bronze);
            if (checksum == await GetChecksumAsync(checksumLocation, cancellationToken))
                continue;

            var properties = _parser.Parse(htmlContent, collection.ParserSchema);

            string bronzeFileLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);
            await _fileWriter.AsJsonAsync(bronzeFileLocation, properties, cancellationToken);
            await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);
        }
    }

    public async Task RunTransformer(Collection collection, CancellationToken cancellationToken)
    {
        foreach (string url in collection.Urls)
        {
            string bronzeFileLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);

            var bronze = await _fileReader.ReadJsonAsync<Data>(bronzeFileLocation, cancellationToken);

            string checksum = CalculateChecksum(bronze, collection.TransformerSchema);
            string checksumLocation = _locator.GetChecksumLocation(collection.Name, url, Medallion.Silver);
            if (checksum == await GetChecksumAsync(checksumLocation, cancellationToken))
                continue;

            var silver = _transformer.Transform(bronze, collection.TransformerSchema);

            string silverFileLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Silver);
            await _fileWriter.AsJsonAsync(silverFileLocation, silver, cancellationToken);
            await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);
        }
    }

    async Task<string> GetChecksumAsync(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }

    string CalculateChecksum(params object[] objects)
    {
        var hashes = objects.Select(x => _hasher.GetSha256HashAsHex(JsonSerializer.Serialize(x)));
        return _hasher.GetSha256HashAsHex(string.Join("", hashes));
    }
}
