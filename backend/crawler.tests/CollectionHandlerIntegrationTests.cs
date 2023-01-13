using common;
using common.Silver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OpenQA.Selenium.Chrome;
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
    [InlineData("makeup-shampoo-urls")]
    [InlineData("makeup-shampoo-variants")]
    public async Task CollectionHandler_ShouldHandleCollections(string collectionName)
    {
        var builder = new ServiceCollection()
            .AddLogging(x => x.AddProvider(new XunitLoggerProvider(_testOutputHelper)))
            .AddCrawler();
        var services = builder.BuildServiceProvider();

        var factory = services.GetRequiredService<CollectionFactory>();
        var handler = services.GetRequiredService<CollectionRunner>();

        var collection = await factory.GetSingleAsync(collectionName, CancellationToken.None);
        await handler.RunLoader(collection, CancellationToken.None);
        await handler.RunParser(collection, CancellationToken.None);
        await handler.RunTransformer(collection, CancellationToken.None);
    }

    [Theory]
    [InlineData("makeup-shampoo-urls")]
    [InlineData("makeup-shampoo-variants")]
    public async Task ParallelCollectionRunner_ShouldHandleCollections(string collectionName)
    {
        var builder = new ServiceCollection()
            .AddLogging(x => x.AddProvider(new XunitLoggerProvider(_testOutputHelper)))
            .AddCrawler();
        var services = builder.BuildServiceProvider();

        var factory = services.GetRequiredService<CollectionFactory>();
        var handler = services.GetRequiredService<ParallelCollectionRunner>();

        var collection = await factory.GetSingleAsync(collectionName, CancellationToken.None);
        await handler.RunAsync(collection, CancellationToken.None);
    }

    [Fact]
    public void Selenium()
    {
        var chromeOptions = new ChromeOptions();

        using var abrowser = new ChromeDriver(chromeOptions);
        abrowser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=0");

        using var bbrowser = new ChromeDriver(chromeOptions);
        bbrowser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=36");

        using var cbrowser = new ChromeDriver(chromeOptions);
        cbrowser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=72");
    }

    [Fact]
    public void Parallelize()
    {
        var items = Enumerable.Range(0, 10);
        foreach (var batch in items.Batch(5))
        {
            Parallel.ForEach(batch, item =>
            {
                Thread.Sleep(1000);
                _testOutputHelper.WriteLine(item.ToString());
            });
            _testOutputHelper.WriteLine("batch");
        }
    }

    [Fact]
    public void Grouping()
    {
        var properties = new List<Property>
        {
            new() { Name = "Test1", Values = new[] { 1, }.Cast<object>().ToList() },
            new() { Name = "Test1", Values = new[] { 2, }.Cast<object>().ToList() },
            new() { Name = "Test2", Values = new[] { 3 }.Cast<object>().ToList() },
        };

        var grouped = properties
            .GroupBy(x => x.Name)
            .Select(x => new Property { Name = x.Key, Values = x.Select(y => y.Values).Cast<object>().ToList() });

        _testOutputHelper.WriteLine(grouped.Dump());
    }

    [Fact]
    public async Task DependencyInjection()
    {
        var builder = new ServiceCollection()
            .AddLogging()
            .AddCrawler();
        await using var services = builder.BuildServiceProvider();

        services.GetRequiredService<ParallelCollectionRunner>();
    }
}
