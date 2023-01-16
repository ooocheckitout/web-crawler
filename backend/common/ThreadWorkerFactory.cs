using common.Threads;
using Microsoft.Extensions.Logging;

namespace common;

public class ThreadWorkerFactory
{
    readonly AppOptions _options;
    readonly ILoggerFactory _loggerFactory;

    public ThreadWorkerFactory(AppOptions options, ILoggerFactory loggerFactory)
    {
        _options = options;
        _loggerFactory = loggerFactory;
    }

    public MultiThreadWorker GetMultiThreadWorker()
    {
        return new MultiThreadWorker(_options.NumberOfWorkerThreads, _loggerFactory.CreateLogger<MultiThreadWorker>());
    }
}
