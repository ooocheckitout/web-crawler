using Xunit;

namespace crawler.tests;

public class CollectionHandlerIntegrationTests
{
    [Theory]
    [InlineData("minfin-petrol-areas")]
    [InlineData("minfin-petrol-prices")]
    public async Task CollectionHandler_ShouldHandleCollections(string collectionName)
    {
        const string collectionsRoot = @"D:\code\web-crawler\collections";
        var hasher = new Hasher();
        var locator = new CollectionLocator(collectionsRoot, hasher);
        var fileReader = new FileReader();
        var factory = new CollectionFactory(locator, fileReader);
        var fileWriter = new FileWriter();
        var handler = new CollectionHandler(
            locator, new WebDownloader(new HttpClient(), fileWriter), fileReader, new Parser(), fileWriter, hasher);

        var collection = await factory.GetSingleAsync(collectionName);
        await handler.HandleAsync(collection);
    }
}
