using common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = new ServiceCollection()
    .AddLogging(x => x.AddConsole())
    .AddCrawler();
var services = builder.BuildServiceProvider();

var factory = services.GetRequiredService<CollectionFactory>();
var handler = services.GetRequiredService<CollectionRunner>();

// foreach (var collection in await factory.GetAllAsync(CancellationToken.None))
{
    var collection = await factory.GetSingleAsync("makeup-shampoo-variants", CancellationToken.None);
    await handler.RunLoader(collection, CancellationToken.None);
    await handler.RunParser(collection, CancellationToken.None);
    await handler.RunTransformer(collection, CancellationToken.None);
}
