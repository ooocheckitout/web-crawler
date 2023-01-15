using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace crawler.tests;

public class XunitLogger : ILogger
{
    readonly ITestOutputHelper _testOutputHelper;
    readonly string _categoryName;

    public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _testOutputHelper.WriteLine($"{DateTime.Now.ToLongTimeString()} {_categoryName} [{eventId}] {formatter(state, exception)}");
        if (exception is not null)
            _testOutputHelper.WriteLine(exception.ToString());
    }

    class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
