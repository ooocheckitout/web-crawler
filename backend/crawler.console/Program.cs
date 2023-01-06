using common;
using common.Bronze;
using common.Silver;

const string collectionsRoot = @"D:\code\web-crawler\collections";

var hasher = new Hasher();
var locator = new CollectionLocator(collectionsRoot, hasher);
var fileReader = new FileReader();
var factory = new CollectionFactory(locator, fileReader);
var fileWriter = new FileWriter();
var handler = new CollectionRunner(
    locator, new WebDownloader(new HttpClient(), fileWriter), fileReader, new Parser(), fileWriter, hasher, new Transformer());

foreach (var collection in await factory.GetAllAsync(CancellationToken.None))
{
    await handler.RunLoader(collection, CancellationToken.None);
    await handler.RunParser(collection, CancellationToken.None);
}
