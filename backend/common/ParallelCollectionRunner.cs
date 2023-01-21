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
    readonly DownloadExecutor _downloadExecutor;
    readonly ParseExecutor _parseExecutor;
    readonly TransformExecutor _transformExecutor;
    readonly CollectionLocator _locator;
    readonly FileWriter _fileWriter;
    readonly Hasher _hasher;
    readonly AppOptions _options;
    readonly MultiThreadWorker _threadWorker;
    readonly ILogger<ParallelCollectionRunner> _logger;

    public ParallelCollectionRunner(
        DownloadExecutor downloadExecutor,
        ParseExecutor parseExecutor,
        TransformExecutor transformExecutor,
        CollectionLocator locator,
        FileWriter fileWriter,
        Hasher hasher,
        AppOptions options,
        MultiThreadWorker threadWorker,
        ILogger<ParallelCollectionRunner> logger)
    {
        _downloadExecutor = downloadExecutor;
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
        using var scope = _logger.BeginScope("scope.collection", collection.Name);

        string lockFileLocation = _locator.GetLockFileLocation(collection.Name);
        if (File.Exists(lockFileLocation))
        {
            _logger.LogInformation("Lock file is present for collection {collectionName}. Skipping...", collection.Name);
            return;
        }

        try
        {
            await _fileWriter.AsTextAsync(lockFileLocation, "", cancellationToken);

            var totalStopWatch = Stopwatch.StartNew();
            var componentsStringBuilder = new StringBuilder();
            foreach (var batch in collection.Urls.Batch(_options.BatchSize).Select(x => x.ToList()))
            {
                var batchStopWatch = Stopwatch.StartNew();

                var actions = batch.Select(x => new Func<Task>(() => ProcessSingleUrlAsync(collection, x, cancellationToken))).ToArray();
                await _threadWorker.ExecuteManyAsync(actions);

                foreach (string url in batch)
                    componentsStringBuilder.Append($"{_hasher.GetSha256HashAsHex(url)} {url} {Environment.NewLine}");

                string componentsLocation = _locator.GetComponentsFileLocation(collection.Name);
                await _fileWriter.AsTextAsync(componentsLocation, componentsStringBuilder.ToString(), cancellationToken);

                _logger.LogInformation(
                    "Processed {count} items from {collection} collection in {totalElapsed} with last batch in {batchElapsed}",
                    batch.Count, collection.Name, totalStopWatch.Elapsed, batchStopWatch.Elapsed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during processing of {collection} collection!", collection.Name);
        }
        finally
        {
            File.Delete(lockFileLocation);
        }
    }

    async Task ProcessSingleUrlAsync(Collection collection, string url, CancellationToken cancellationToken)
    {
        using var x = _logger.BeginScope("scope.url", url);

        try
        {
            string htmlLocation = _locator.GetHtmlFileLocation(collection.Name, url);
            await _downloadExecutor.LoadContentAsync(url, htmlLocation, cancellationToken);

            string bronzeLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);
            string bronzeChecksumLocation = _locator.GetChecksumFileLocation(collection.Name, url, Medallion.Bronze);
            await _parseExecutor.ParseAsync(htmlLocation, bronzeLocation, bronzeChecksumLocation, collection.ParserSchema, cancellationToken);

            string silverLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Silver);
            string silverChecksumLocation = _locator.GetChecksumFileLocation(collection.Name, url, Medallion.Silver);
            await _transformExecutor.TransformAsync(bronzeLocation, silverLocation, silverChecksumLocation, collection.TransformerSchema, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {url} for collection {collection}", url, collection.Name);
        }
    }
}
