using System.Diagnostics;
using System.Text;
using common.Collections;
using common.Executors;
using common.Threads;
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
    readonly AppOptions _options;
    readonly MultiThreadWorker _threadWorker;
    readonly ILogger<ParallelCollectionRunner> _logger;

    public ParallelCollectionRunner(
        LoadExecutor loadExecutor,
        ParseExecutor parseExecutor,
        TransformExecutor transformExecutor,
        CollectionLocator locator,
        FileWriter fileWriter,
        Hasher hasher,
        AppOptions options,
        MultiThreadWorker threadWorker,
        ILogger<ParallelCollectionRunner> logger)
    {
        _loadExecutor = loadExecutor;
        _parseExecutor = parseExecutor;
        _transformExecutor = transformExecutor;
        _locator = locator;
        _fileWriter = fileWriter;
        _hasher = hasher;
        _options = options;
        _threadWorker = threadWorker;
        _logger = logger;
    }

    public async Task RunAsync(Collection collection, CancellationToken cancellationToken)
    {
        string lockFileLocation = _locator.GetLockFileLocation(collection.Name);
        if (File.Exists(lockFileLocation))
        {
            _logger.LogInformation("Lock file is present for collection {collectionName}. Skipping...", collection.Name);
            return;
        }

        await _fileWriter.AsTextAsync(lockFileLocation, "", cancellationToken);

        var sw = Stopwatch.StartNew();
        var sb = new StringBuilder();
        foreach (var itemWithIndex in collection.Urls.Batch(_options.BatchSize).WithIndex())
        {
            var batchSw = Stopwatch.StartNew();

            var actions = itemWithIndex.Item.Select(x => new Func<Task>(() => ProcessAsync(collection, x, cancellationToken))).ToArray();
            await _threadWorker.ExecuteManyAsync(actions);

            foreach (string url in itemWithIndex.Item)
            {
                sb.Append($"{_hasher.GetSha256HashAsHex(url)} {url} {Environment.NewLine}");
            }

            string componentsLocation = _locator.GetComponentsFileLocation(collection.Name);
            await _fileWriter.AsTextAsync(componentsLocation, sb.ToString(), cancellationToken);

            _logger.LogInformation(
                "Processed {count} items in {elapsed} and {elapsedPerBatch} per batch for {collection}",
                (itemWithIndex.Index + 1) * _options.BatchSize, sw.Elapsed, batchSw.Elapsed, collection.Name);
        }

        File.Delete(lockFileLocation);
    }

    async Task ProcessAsync(Collection collection, string url, CancellationToken cancellationToken)
    {
        using var _ = _logger.BeginScope(url);

        string htmlLocation = _locator.GetHtmlFileLocation(collection.Name, url);
        await _loadExecutor.LoadContentAsync(url, htmlLocation, cancellationToken);

        string bronzeLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);
        string bronzeChecksumLocation = _locator.GetChecksumFileLocation(collection.Name, url, Medallion.Bronze);
        await _parseExecutor.ParseAsync(htmlLocation, bronzeLocation, bronzeChecksumLocation, collection.ParserSchema, cancellationToken);

        string silverLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Silver);
        string silverChecksumLocation = _locator.GetChecksumFileLocation(collection.Name, url, Medallion.Silver);
        await _transformExecutor.TransformAsync(bronzeLocation, silverLocation, silverChecksumLocation, collection.TransformerSchema, cancellationToken);
    }
}
