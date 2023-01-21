using Microsoft.Extensions.Logging;

namespace common;

public static class LoggerExtension
{
    public static IDisposable? BeginScope(this ILogger logger, string key, object value)
    {
        return logger.BeginScope(new Dictionary<string, object> { { key, value } });
    }
}
