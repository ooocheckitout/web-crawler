using common;
using common.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = new ServiceCollection()
    .AddLogging(x => x
        .AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "HH:mm:ss.fff ";
        })
        .AddFile("app.log", append: true)
    )
    .AddCrawler();
await using var services = builder.BuildServiceProvider();

var factory = services.GetRequiredService<CollectionFactory>();
var handler = services.GetRequiredService<ParallelCollectionRunner>();
var logger = services.GetRequiredService<ILogger<Program>>();

if (args.Length == 0)
{
    logger.LogInformation("Running all collections");
    foreach (var collection in await factory.GetAllAsync(CancellationToken.None))
    {
        await handler.RunAsync(collection, CancellationToken.None);
    }
}
else if (args.Length == 1)
{
    logger.LogInformation("Running {collection} collection", args[0]);
    var collection = await factory.GetSingleAsync(args[0], CancellationToken.None);
    await handler.RunAsync(collection, CancellationToken.None);
}

logger.LogInformation("Finished running collections");
