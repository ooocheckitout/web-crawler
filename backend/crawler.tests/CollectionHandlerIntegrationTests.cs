using System.Diagnostics;
using common;
using common.Silver;
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
    public async Task Selenium_DriverPerPage()
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
            new() {Name = "Test1", Values = new[] {1,}.Cast<object>().ToList()},
            new() {Name = "Test1", Values = new[] {2,}.Cast<object>().ToList()},
            new() {Name = "Test2", Values = new[] {3}.Cast<object>().ToList()},
        };

        var grouped = properties
            .GroupBy(x => x.Name)
            .Select(x => new Property {Name = x.Key, Values = x.Select(y => y.Values).Cast<object>().ToList()});

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

    [Theory]
    [InlineData(10, 10)]
    public async Task Parallel_Tasks(int numberOfItems, int numberOfHandlers)
    {
        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddProvider(new XunitLoggerProvider(_testOutputHelper)));
        var services = builder.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<CollectionHandlerIntegrationTests>>();

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
        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddProvider(new XunitLoggerProvider(_testOutputHelper)));
        var services = builder.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<CollectionHandlerIntegrationTests>>();

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
        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddProvider(new XunitLoggerProvider(_testOutputHelper)));
        var services = builder.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<CollectionHandlerIntegrationTests>>();

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
    public async Task Parallel_ThreadWorker(int numberOfItems, int numberOfHandlers)
    {
        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddProvider(new XunitLoggerProvider(_testOutputHelper)));
        var services = builder.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<CollectionHandlerIntegrationTests>>();

        var item = 0;
        using var broker = new LeaseBroker<int>(() => item++, numberOfHandlers, logger);

        using var workers = new LeaseBroker<ThreadWorker>(() => new ThreadWorker(logger), numberOfHandlers, logger);
        var tasks = Enumerable.Range(0, numberOfItems).Select(x =>
        {
            var lease = workers.TakeLease();
            return lease.Value.ExecuteAsync(() =>
            {
                Thread.Sleep(3000);
                workers.ReturnLease(lease);
            });
        });
        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10, 10)]
    public async Task Parallel_MultiThreadWorker(int numberOfItems, int numberOfHandlers)
    {
        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddProvider(new XunitLoggerProvider(_testOutputHelper)));
        var services = builder.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<CollectionHandlerIntegrationTests>>();

        var item = 0;
        using var broker = new LeaseBroker<int>(() => item++, numberOfHandlers, logger);

        using var multiThreadWorker = new MultiThreadWorker(numberOfHandlers, logger);
        var actions = Enumerable.Range(0, numberOfItems).Select(_ => new Action(() => Thread.Sleep(3000))).ToArray();
        await multiThreadWorker.ExecuteManyAsync(actions);
    }

    [Fact]
    public async Task X()
    {
        // 3 items 5 drivers 45 sec
        // 00:00:19.2460926
        // 00:00:20.1625085
        // 00:00:20.2111937

        // 6 items 5 drivers 1 min
        // 00:00:34.6526458
        // 00:00:34.7323176
        // 00:00:34.8276810
        // 00:00:03.2209547
        // 00:00:41.7234537
        // 00:00:10.1377826

        // 9 items 5 drivers 58 sec
        // 00:00:24.7881836
        // 00:00:25.0653232
        // 00:00:25.4892406
        // 00:00:25.6480990
        // 00:00:03.6729242
        // 00:00:03.1596662
        // 00:00:11.7588896
        // 00:00:13.0247375
        // 00:00:10.2267391

        // 12 items 5 drivers 1 min 20 sec
        // 00:00:39.6596872
        // 00:00:39.8322249
        // 00:00:42.3287459
        // 00:00:42.4072463
        // 00:00:03.1862283
        // 00:00:03.1410544
        // 00:00:03.7924767
        // 00:00:04.3668256
        // 00:00:04.0291749
        // 00:00:03.3626974
        // 00:00:03.4542302
        // 00:00:15.3015256

        // 12 items 10 drivers 2 min 20 sec
        // 00:00:30.0038378
        // 00:00:32.7081606
        // 00:00:04.6861130
        // 00:00:03.6270446
        // 00:00:03.6349488
        // 00:00:03.2640606
        // 00:00:40.6982663
        // 00:00:41.0459323
        // 00:00:03.1451681
        // 00:00:03.1109537
        // 00:00:03.5854824
        // 00:00:12.8146407


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

        var builder = new ServiceCollection()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Debug).AddProvider(new XunitLoggerProvider(_testOutputHelper)));
        var services = builder.BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<CollectionHandlerIntegrationTests>>();

        using var drivers = new LeaseBroker<ChromeDriver>(() => new ChromeDriver(chromeOptions), 3, logger);

        using var multiThreadWorker = new MultiThreadWorker(3, logger);
        var actions = duplicatedItems.Select(item => new Action(() =>
        {
            var lease = drivers.TakeLease();
            var browser = lease.Value;

            var sw = Stopwatch.StartNew();
            browser.Navigate().GoToUrl(item.Url);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            browser
                .FindElement(By.XPath(xpath))
                .GetDomAttribute("href").Should().Be(item.Href);
            _testOutputHelper.WriteLine(sw.Elapsed.ToString());

            drivers.ReturnLease(lease);
        })).ToArray();
        await multiThreadWorker.ExecuteManyAsync(actions);
    }
}
