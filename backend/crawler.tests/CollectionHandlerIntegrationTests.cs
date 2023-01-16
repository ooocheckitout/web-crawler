using System.Diagnostics;
using common;
using common.Collections;
using common.Silver;
using common.Threads;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace crawler.tests;

public class CollectionHandlerIntegrationTests
{
    readonly ITestOutputHelper _testOutputHelper;
    readonly ServiceProvider _services;

    public CollectionHandlerIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Information).AddProvider(new XunitLoggerProvider(_testOutputHelper)))
            .AddCrawler();
        _services = builder.BuildServiceProvider();
    }

    [Theory]
    [InlineData("makeup-shampoo-urls")]
    [InlineData("makeup-shampoo-variants")]
    public async Task CollectionHandler_ShouldHandleCollections(string collectionName)
    {
        var factory = _services.GetRequiredService<CollectionFactory>();
        var handler = _services.GetRequiredService<CollectionRunner>();

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

        var factory = _services.GetRequiredService<CollectionFactory>();
        var handler = _services.GetRequiredService<ParallelCollectionRunner>();

        var collection = await factory.GetSingleAsync(collectionName, CancellationToken.None);
        await handler.RunAsync(collection, CancellationToken.None);
    }

    [Fact]
    public void Selenium_DriverPerPage()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");

        using var abrowser = new ChromeDriver(chromeOptions);
        abrowser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=0");
        abrowser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/182877/");

        using var bbrowser = new ChromeDriver(chromeOptions);
        bbrowser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=36");
        bbrowser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/974371/");

        using var cbrowser = new ChromeDriver(chromeOptions);
        cbrowser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=72");
        cbrowser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/936589/");
    }

    [Fact]
    public async Task Selenium_SingleDriver()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");

        using var browser = new ChromeDriver(chromeOptions);
        var delay = TimeSpan.FromSeconds(3);

        browser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=0");
        await Task.Delay(delay);
        browser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/182877/");

        browser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=36");
        await Task.Delay(delay);
        browser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/974371/");

        browser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=72");
        await Task.Delay(delay);
        browser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/936589/");
    }

    [Fact]
    public void Selenium_LoadTime()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");

        using var browser = new ChromeDriver(chromeOptions);

        var sw = Stopwatch.StartNew();
        browser.Navigate().GoToUrl("https://makeup.com.ua/ua/categorys/22806/#offset=0");
        browser
            .FindElement(By.XPath("/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a"))
            .GetDomAttribute("href").Should().Be("/ua/product/182877/");
        _testOutputHelper.WriteLine(sw.Elapsed.Dump());
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
        _services.GetRequiredService<ParallelCollectionRunner>();
    }

    [Theory]
    [InlineData(10, 10)]
    public async Task Parallel_Tasks(int numberOfItems, int numberOfHandlers)
    {
        var logger = _services.GetRequiredService<ILogger<LeaseBroker<int>>>();

        var item = 0;
        using var broker = new LeaseBroker<int>(() => item++, numberOfHandlers, logger);

        var tasks = Enumerable.Range(0, numberOfItems).Select(_ => Task.Run(() =>
        {
            var lease = broker.TakeLease();
            Thread.Sleep(3000);
            broker.ReturnLease(lease);
        }));
        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10, 10)]
    public void Parallel_ForEach(int numberOfItems, int numberOfHandlers)
    {
        var logger = _services.GetRequiredService<ILogger<LeaseBroker<int>>>();

        var item = 0;
        using var broker = new LeaseBroker<int>(() => item++, numberOfHandlers, logger);

        Parallel.ForEach(Enumerable.Range(0, numberOfItems), _ =>
        {
            var lease = broker.TakeLease();
            Thread.Sleep(3000);
            broker.ReturnLease(lease);
        });
    }

    [Theory]
    [InlineData(10, 10)]
    public void Parallel_Threads(int numberOfItems, int numberOfHandlers)
    {
        var logger = _services.GetRequiredService<ILogger<LeaseBroker<int>>>();

        var item = 0;
        using var broker = new LeaseBroker<int>(() => item++, numberOfHandlers, logger);

        var workers = Enumerable.Range(0, numberOfItems).Select(_ => new Thread(() =>
        {
            var lease = broker.TakeLease();
            Thread.Sleep(3000);
            broker.ReturnLease(lease);
        })).ToList();
        workers.ForEach(x => x.Start());
        workers.ForEach(x => x.Join());
    }

    [Theory]
    [InlineData(10, 10)]
    public async Task Parallel_MultiThreadWorker(int numberOfItems, int numberOfHandlers)
    {
        var logger = _services.GetRequiredService<ILogger<LeaseBroker<int>>>();
        var workerLogger = _services.GetRequiredService<ILogger<MultiThreadWorker>>();

        var item = 0;
        using var broker = new LeaseBroker<int>(() => item++, numberOfHandlers, logger);

        using var multiThreadWorker = new MultiThreadWorker(numberOfHandlers, workerLogger);
        var actions = Enumerable.Range(0, numberOfItems).Select(_ => new Func<Task>(() => Task.Delay(3000))).ToArray();
        await multiThreadWorker.ExecuteManyAsync(actions);
    }

    [Fact]
    public async Task X()
    {
        // 3 items 3 drivers 22 sec
        // 6 items 3 drivers 32 sec
        // 9 items 3 drivers 38 sec


        const string xpath = "/html/body/div/div[1]/div/div/div[2]/div[1]/div[1]/div/div[9]/ul/li/div[2]/a";
        var items = new[]
        {
            new
            {
                Url = "https://makeup.com.ua/ua/categorys/22806/#offset=0",
                Href = "/ua/product/182877/"
            },
            new
            {
                Url = "https://makeup.com.ua/ua/categorys/22806/#offset=36",
                Href = "/ua/product/34319/"
            },
            new
            {
                Url = "https://makeup.com.ua/ua/categorys/22806/#offset=72",
                Href = "/ua/product/35695/"
            }
        };
        var duplicatedItems = items.Concat(items).Concat(items);

        var chromeOptions = new ChromeOptions();
        // chromeOptions.AddArguments("headless");

        var logger = _services.GetRequiredService<ILogger<LeaseBroker<ChromeDriver>>>();
        var workerLogger = _services.GetRequiredService<ILogger<MultiThreadWorker>>();

        using var drivers = new LeaseBroker<ChromeDriver>(() => new ChromeDriver(chromeOptions), 3, logger);

        using var multiThreadWorker = new MultiThreadWorker(3, workerLogger);
        var actions = duplicatedItems.Select(item => new Func<Task>(async () =>
        {
            var lease = drivers.TakeLease();
            var browser = lease.Value;

            var sw = Stopwatch.StartNew();
            browser.Navigate().GoToUrl(item.Url);
            await Task.Delay(3000);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            browser
                .FindElement(By.XPath(xpath))
                .GetDomAttribute("href").Should().Be(item.Href);
            _testOutputHelper.WriteLine(sw.Elapsed.ToString());

            drivers.ReturnLease(lease);
        })).ToArray();
        await multiThreadWorker.ExecuteManyAsync(actions);
    }

    [Fact]
    public async Task Hasher()
    {
        var factory = _services.GetRequiredService<CollectionFactory>();
        var hasher = _services.GetRequiredService<Hasher>();

        var collection = await factory.GetSingleAsync("makeup-shampoo-urls", CancellationToken.None);
        var hashes = collection.Urls.Select(x => hasher.GetSha256HashAsHex(x)).ToList();

        _testOutputHelper.WriteLine(collection.Urls.Count().ToString());
        _testOutputHelper.WriteLine(collection.Urls.Distinct().Count().ToString());
        _testOutputHelper.WriteLine(hashes.Count.ToString());
        _testOutputHelper.WriteLine(hashes.Distinct().Count().ToString());
    }

    [Fact]
    public async Task LogAnalytics()
    {
        var fileReader = _services.GetRequiredService<FileReader>();

        const string logLocation = @"D:\code\web-crawler\backend\crawler.console\bin\Debug\net6.0\log-20230116.txt";
        string logContents = await fileReader.ReadTextAsync(logLocation, CancellationToken.None);
        var logs = logContents
            .Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                string[] parts = line.Split("\t");
                return new { Timestamp = parts[0], LogLevel = parts[1], Logger = parts[2], Message = parts[4] };
            })
            .ToList();


        _testOutputHelper.WriteLine(logs.Count.ToString());
    }
}
