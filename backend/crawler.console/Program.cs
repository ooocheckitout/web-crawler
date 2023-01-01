const string collectionsRoot = @"D:\code\web-crawler\collections";

var hasher = new Hasher();
var locator = new CollectionLocator(collectionsRoot, hasher);
var fileReader = new FileReader();
var factory = new CollectionFactory(locator, fileReader);
var fileWriter = new FileWriter();
var handler = new CollectionHandler(
    locator, new WebDownloader(new HttpClient(), fileWriter), fileReader, new Parser(), fileWriter, hasher);

foreach (var collection in await factory.GetAllAsync())
{
    await handler.HandleAsync(collection);
}
