using common;
using common.Bronze;
using common.Silver;
using Xunit;

namespace crawler.tests;

public class CollectionHandlerIntegrationTests
{
    [Theory]
    // [InlineData("minfin-petrol-regions")]
    // [InlineData("minfin-petrol-prices")]
    // [InlineData("tailwind-color-palette")]
    [InlineData("makeup-shampoo-urls")]
    // [InlineData("makeup-shampoo-details")]
    public async Task CollectionHandler_ShouldHandleCollections(string collectionName)
    {
        const string collectionsRoot = @"D:\code\web-crawler\collections";
        var hasher = new Hasher();
        var locator = new CollectionLocator(collectionsRoot, hasher);
        var fileReader = new FileReader();
        var factory = new CollectionFactory(locator, fileReader);
        var fileWriter = new FileWriter();
        var handler = new CollectionRunner(
            locator, new WebDownloader(new HttpClient(), fileWriter), fileReader, new Parser(), fileWriter, hasher, new Transformer());

        var collection = await factory.GetSingleAsync(collectionName, CancellationToken.None);
        await handler.RunLoader(collection, CancellationToken.None);
        await handler.RunParser(collection, CancellationToken.None);
        await handler.RunTransformer(collection, CancellationToken.None);
    }
}
