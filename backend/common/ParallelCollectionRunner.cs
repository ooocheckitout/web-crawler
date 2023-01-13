using common.Bronze;
using common.Silver;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace common;

public class ParallelCollectionRunner
{
    readonly CollectionLocator _locator;
    readonly SeleniumDownloader _downloader;
    readonly FileWriter _fileWriter;
    readonly FileReader _fileReader;
    readonly Hasher _hasher;
    readonly Parser _parser;
    readonly Transformer _transformer;
    readonly ChecksumCalculator _checksumCalculator;
    readonly ILogger<ParallelCollectionRunner> _logger;
    const int BatchSize = 10;

    public ParallelCollectionRunner(
        CollectionLocator locator,
        SeleniumDownloader downloader,
        FileWriter fileWriter,
        FileReader fileReader,
        Hasher hasher,
        Parser parser,
        Transformer transformer,
        ChecksumCalculator checksumCalculator,
        ILogger<ParallelCollectionRunner> logger)
    {
        _locator = locator;
        _downloader = downloader;
        _fileWriter = fileWriter;
        _fileReader = fileReader;
        _hasher = hasher;
        _parser = parser;
        _transformer = transformer;
        _checksumCalculator = checksumCalculator;
        _logger = logger;
    }

    public async Task RunAsync(Collection collection, CancellationToken cancellationToken)
    {
        foreach (var batch in collection.Urls.Batch(BatchSize))
        {
            await Parallel.ForEachAsync(batch, cancellationToken, async (url, token) =>
            {
                string htmlContent = await LoadContentsAsync(collection.Name, url, token);
                var bronze = await ParseAsync(collection.Name, url, collection.ParserSchema, htmlContent, token);
                var silver = await TransformAsync(collection.Name, url, collection.TransformerSchema, bronze.ToList(), token);
            });

            await Task.Delay(1000, cancellationToken);
        }
    }

    async Task<string> LoadContentsAsync(string collectionName, string url, CancellationToken cancellationToken)
    {
        string htmlLocation = _locator.GetHtmlLocation(collectionName, url);

        if (File.Exists(htmlLocation))
            return await _fileReader.ReadTextAsync(htmlLocation, cancellationToken);

        _logger.LogInformation("Downloading from {url} to {htmlLocation}", url, htmlLocation);
        string htmlContent = await _downloader.DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);

        return htmlContent;
    }

    async Task<IEnumerable<Property>> ParseAsync(string collectionName, string url, ParserSchema schema, string htmlContent, CancellationToken cancellationToken)
    {
        string checksum = _checksumCalculator.GetParserChecksum(schema, htmlContent);
        string checksumLocation = _locator.GetChecksumLocation(collectionName, url, Medallion.Bronze);

        string dataFileLocation = _locator.GetDataFileLocation(collectionName, url, Medallion.Bronze);
        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
            return await _fileReader.ReadJsonAsync<IEnumerable<Property>>(dataFileLocation, cancellationToken);

        _logger.LogInformation("Parsing content from {url}", url);
        var bronze = _parser.Parse(htmlContent, schema);
        await _fileWriter.AsJsonAsync(dataFileLocation, bronze, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);

        return bronze;
    }

    async Task<IEnumerable<Property>> TransformAsync(
        string collectionName, string url, TransformerSchema schema, ICollection<Property> bronze, CancellationToken cancellationToken)
    {
        string checksum = _checksumCalculator.GetTransformerChecksum(schema, bronze);
        string checksumLocation = _locator.GetChecksumLocation(collectionName, url, Medallion.Silver);

        string dataFileLocation = _locator.GetDataFileLocation(collectionName, url, Medallion.Silver);
        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
            return await _fileReader.ReadJsonAsync<IEnumerable<Property>>(dataFileLocation, cancellationToken);

        _logger.LogInformation("Transforming content from {url}", url);
        var silver = _transformer.Transform(bronze, schema);
        await _fileWriter.AsJsonAsync(dataFileLocation, silver, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);

        return silver;
    }

    async Task<string> ReadChecksum(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
