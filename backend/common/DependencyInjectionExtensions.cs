using common;
using common.Bronze;
using common.Silver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        builder.AddTransient<ParallelCollectionRunner>();

        return builder;
    }
}
