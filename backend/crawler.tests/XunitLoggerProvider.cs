using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace crawler.tests;

public class XunitLoggerProvider : ILoggerProvider
{
    readonly ITestOutputHelper _testOutputHelper;

    public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName) => new XunitLogger(_testOutputHelper, categoryName);

    public void Dispose()
    {
    }
}
