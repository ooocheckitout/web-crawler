using common;
using common.Bronze;
using common.Silver;
using crawler.tests;
using Microsoft.Extensions.Logging;

const string collectionsRoot = @"D:\code\web-crawler\collections";

var hasher = new Hasher();
var locator = new CollectionLocator(collectionsRoot, hasher);
var fileReader = new FileReader();
var factory = new CollectionFactory(locator, fileReader);
var fileWriter = new FileWriter();
using var downloader = new SeleniumDownloader();
var loggerFactory = new LoggerFactory();
var handler = new CollectionRunner(locator, downloader, fileReader, new Parser(), fileWriter, hasher, new Transformer(), loggerFactory.CreateLogger<CollectionRunner>());

foreach (var collection in await factory.GetAllAsync(CancellationToken.None))
{
    await handler.RunLoader(collection, CancellationToken.None);
    await handler.RunParser(collection, CancellationToken.None);
}
