using common;
using common.Bronze;
using common.Silver;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace crawler.tests;

public class CollectionHandlerIntegrationTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public CollectionHandlerIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    // [InlineData("minfin-petrol-regions")]
    // [InlineData("minfin-petrol-prices")]
    // [InlineData("tailwind-color-palette")]
    // [InlineData("makeup-shampoo-urls")]
    [InlineData("makeup-shampoo-variants")]
    // [InlineData("makeup-shampoo-details")]
    public async Task CollectionHandler_ShouldHandleCollections(string collectionName)
    {
        const string collectionsRoot = @"D:\code\web-crawler\collections";
        var hasher = new Hasher();
        var locator = new CollectionLocator(collectionsRoot, hasher);
        var fileReader = new FileReader();
        var factory = new CollectionFactory(locator, fileReader);
        var fileWriter = new FileWriter();
        using var downloader = new SeleniumDownloader();
        var loggerFactory = LoggerFactory.Create(builder => { builder.AddProvider(new XunitLoggerProvider(_testOutputHelper)); });
        var handler = new CollectionRunner(
            locator, downloader, fileReader, new Parser(), fileWriter, hasher, new Transformer(), loggerFactory.CreateLogger<CollectionRunner>());

        var collection = await factory.GetSingleAsync(collectionName, CancellationToken.None);
        await handler.RunLoader(collection, CancellationToken.None);
        await handler.RunParser(collection, CancellationToken.None);
        await handler.RunTransformer(collection, CancellationToken.None);
    }
}
