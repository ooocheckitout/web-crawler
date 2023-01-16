using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace analytics.api.Controllers;

[ApiController]
[Route("collections/{collection}/run")]
public class RunController : ControllerBase, IDisposable
{
    readonly ILogger<RunController> _logger;
    readonly object _locker = new();
    Process? _process;

    public RunController(ILogger<RunController> logger)
    {
        _logger = logger;
    }

    [HttpPut]
    [Route("")]
    public Task Run(string collection, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {collection}", collection);

        lock (_locker)
        {
            if (_process is null)
            {
                _process = Process.Start(@"D:\code\web-crawler\backend\crawler.console\bin\Debug\net6.0\crawler.console.exe", collection);
            }
            else if (_process.HasExited)
            {
                _process = Process.Start(@"D:\code\web-crawler\backend\crawler.console\bin\Debug\net6.0\crawler.console.exe", collection);
            }
            else
            {
                _logger.LogInformation("Still in progress");
            }
        }

        return Task.CompletedTask;
    }

    public void Dispose() => _process?.Dispose();
}
