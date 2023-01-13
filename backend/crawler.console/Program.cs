using common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = new ServiceCollection()
    .AddLogging(x => x.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.UseUtcTimestamp = true;
        options.TimestampFormat = "HH:mm:ss ";
    }))
    .AddCrawler();
await using var services = builder.BuildServiceProvider();

var factory = services.GetRequiredService<CollectionFactory>();
var handler = services.GetRequiredService<ParallelCollectionRunner>();

// foreach (var collection in await factory.GetAllAsync(CancellationToken.None))
{
    var collection = await factory.GetSingleAsync("makeup-shampoo-urls", CancellationToken.None);
    await handler.RunAsync(collection, CancellationToken.None);
}
