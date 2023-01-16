using Microsoft.Extensions.Logging;

namespace common.Executors;

public class DownloadExecutor
{
    readonly FileWriter _fileWriter;
    readonly LeaseBroker<SeleniumDownloader> _downloaders;
    readonly ILogger<DownloadExecutor> _logger;

    public DownloadExecutor(FileWriter fileWriter, LeaseBroker<SeleniumDownloader> downloaders, ILogger<DownloadExecutor> logger)
    {
        _fileWriter = fileWriter;
        _downloaders = downloaders;
        _logger = logger;
    }

    public async Task LoadContentAsync(string url, string htmlLocation, CancellationToken cancellationToken)
    {
        if (File.Exists(htmlLocation))
            return;

        _logger.LogInformation("Downloading from {url} to {htmlLocation}", url, htmlLocation);
        using var downloader = _downloaders.TakeLease();
        string htmlContent = await downloader.Value.DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);
    }
}
