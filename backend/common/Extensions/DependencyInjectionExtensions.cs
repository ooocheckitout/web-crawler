using common.Bronze;
using common.Collections;
using common.Executors;
using common.Silver;
using common.Threads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCrawlerLogging(this IServiceCollection builder)
    {
        string logFileLocation = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "app.log");
        var log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("executionId", Guid.NewGuid())
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(logFileLocation)
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

        return builder.AddLogging(x => x.ClearProviders().AddSerilog(log));
    }

    public static IServiceCollection AddCrawler(this IServiceCollection builder)
    {
        const string collectionsRoot = @"D:\code\web-crawler\collections";
        builder.AddTransient<Hasher>();
        builder.AddTransient<ChecksumCalculator>();
        builder.AddTransient<FileReader>();
        builder.AddTransient<FileWriter>();
        builder.AddTransient<SeleniumDownloader>();
        builder.AddTransient<Parser>();
        builder.AddTransient<Transformer>();
        builder.AddTransient(provider => new CollectionLocator(collectionsRoot, provider.GetRequiredService<Hasher>()));
        builder.AddTransient<CollectionFactory>();
        builder.AddTransient<CollectionRunner>();
        builder.AddTransient<DownloadExecutor>();
        builder.AddTransient<ParseExecutor>();
        builder.AddTransient<TransformExecutor>();
        builder.AddTransient(provider =>
            new MultiThreadWorker(
                provider.GetRequiredService<AppOptions>().NumberOfWorkerThreads,
                provider.GetRequiredService<ILogger<MultiThreadWorker>>()
            )
        );
        builder.AddTransient(_ => new AppOptions
        {
            BatchSize = 50,
            NumberOfSeleniumDownloaders = 5,
            NumberOfWorkerThreads = 5,
            SeleniumPageLoadDelay = TimeSpan.FromSeconds(3),
        });
        builder.AddTransient(provider =>
            new LeaseBroker<SeleniumDownloader>(
                provider.GetRequiredService<SeleniumDownloader>,
                provider.GetRequiredService<AppOptions>().NumberOfSeleniumDownloaders,
                provider.GetRequiredService<ILogger<LeaseBroker<SeleniumDownloader>>>()
            )
        );
        builder.AddTransient<ParallelCollectionRunner>();
        builder.AddTransient<InterprocessCommunicator>();

        return builder;
    }
}
