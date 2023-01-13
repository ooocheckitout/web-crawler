using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace common;

public class ParallelCollectionRunner
{
    readonly LoadExecutor _loadExecutor;
    readonly ParseExecutor _parseExecutor;
    readonly TransformExecutor _transformExecutor;
    readonly CollectionLocator _locator;
    readonly FileWriter _fileWriter;
    readonly Hasher _hasher;
    readonly ILogger<ParallelCollectionRunner> _logger;
    const int BatchSize = 10;

    public ParallelCollectionRunner(
        LoadExecutor loadExecutor,
        ParseExecutor parseExecutor,
        TransformExecutor transformExecutor,
        CollectionLocator locator,
        FileWriter fileWriter,
        Hasher hasher,
        ILogger<ParallelCollectionRunner> logger)
    {
        _loadExecutor = loadExecutor;
        _parseExecutor = parseExecutor;
        _transformExecutor = transformExecutor;
        _locator = locator;
        _fileWriter = fileWriter;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task RunAsync(Collection collection, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        foreach (var itemWithIndex in collection.Urls.Batch(BatchSize).WithIndex())
        {
            var sw = Stopwatch.StartNew();
            await Parallel.ForEachAsync(itemWithIndex.Item, cancellationToken, async (url, token) =>
            {
                using var _ = _logger.BeginScope(url);

                string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);
                await _loadExecutor.LoadContentAsync(url, htmlLocation, token);

                string bronzeLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);
                string bronzeChecksumLocation = _locator.GetChecksumLocation(collection.Name, url, Medallion.Bronze);
                await _parseExecutor.ParseAsync(htmlLocation, bronzeLocation, bronzeChecksumLocation, collection.ParserSchema, token);

                string silverLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Silver);
                string silverChecksumLocation = _locator.GetChecksumLocation(collection.Name, url, Medallion.Silver);
                await _transformExecutor.TransformAsync(bronzeLocation, silverLocation, silverChecksumLocation, collection.TransformerSchema, token);
            });

            foreach (string url in itemWithIndex.Item)
            {
                sb.Append($"{_hasher.GetSha256HashAsHex(url)} {url} {Environment.NewLine}");
            }

            string componentsLocation = _locator.GetComponentsFileLocation(collection.Name);
            await _fileWriter.AsTextAsync(componentsLocation, sb.ToString(), cancellationToken);

            _logger.LogInformation(
                "{collectionName} progress: {processedElements} {elapsed}", collection.Name, (itemWithIndex.Index + 1) * BatchSize, sw.Elapsed);
        }
    }
}
