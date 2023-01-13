using common.Bronze;
using common.Silver;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace common;

public class ParallelCollectionRunner
{
    readonly LoadExecutor _loadExecutor;
    readonly ParseExecutor _parseExecutor;
    readonly TransformExecutor _transformExecutor;
    readonly ILogger<ParallelCollectionRunner> _logger;
    const int BatchSize = 10;

    public ParallelCollectionRunner(
        LoadExecutor loadExecutor,
        ParseExecutor parseExecutor,
        TransformExecutor transformExecutor,
        ILogger<ParallelCollectionRunner> logger)
    {
        _loadExecutor = loadExecutor;
        _parseExecutor = parseExecutor;
        _transformExecutor = transformExecutor;
        _logger = logger;
    }

    public async Task RunAsync(Collection collection, CancellationToken cancellationToken)
    {
        foreach (var itemWithIndex in collection.Urls.Batch(BatchSize).WithIndex())
        {
            await Parallel.ForEachAsync(itemWithIndex.Item, cancellationToken, async (url, token) =>
            {
                string htmlContent = await _loadExecutor.LoadContentsAsync(collection.Name, url, token);
                var bronze = await _parseExecutor.ParseAsync(collection.Name, url, collection.ParserSchema, htmlContent, token);
                var silver = await _transformExecutor.TransformAsync(collection.Name, url, collection.TransformerSchema, bronze.ToList(), token);
            });

            await Task.Delay(1000, cancellationToken);
            _logger.LogInformation("Progress: {ProcessedElements}", itemWithIndex.Index * BatchSize);
        }
    }
}
