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
        builder.AddTransient<LoadExecutor>();
        builder.AddTransient<ParseExecutor>();
        builder.AddTransient<TransformExecutor>();
        builder.AddTransient<ThreadWorker>();
        builder.AddTransient<MultiThreadWorker>();
        builder.AddTransient<AppOptions>(_ => new AppOptions
        {
            BatchSize = 50,
            NumberOfSeleniumDownloaders = 5,
            NumberOfWorkerThreads = 5
        });
        builder.AddTransient<LeaseBroker<SeleniumDownloader>>(provider =>
            new LeaseBroker<SeleniumDownloader>(
                provider.GetRequiredService<SeleniumDownloader>,
                provider.GetRequiredService<AppOptions>().NumberOfSeleniumDownloaders,
                provider.GetRequiredService<ILogger<LeaseBroker<SeleniumDownloader>>>()
            )
        );
        builder.AddTransient<ParallelCollectionRunner>();

        return builder;
    }
}
