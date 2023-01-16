using common.Bronze;
using common.Collections;
using common.Executors;
using common.Silver;
using common.Threads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace common;

public static class DependencyInjectionExtensions
{
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
            NumberOfSeleniumDownloaders = 6,
            NumberOfWorkerThreads = 6,
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
