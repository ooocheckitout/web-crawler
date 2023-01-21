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
        _logger.LogDebug("Start downloading from {url} to {htmlLocation}", url, htmlLocation);

        if (File.Exists(htmlLocation))
        {
            _logger.LogDebug("File already exist. Skip downloading from {url} to {htmlLocation}", url, htmlLocation);
            return;
        }

        using var downloader = _downloaders.TakeLease();
        string htmlContent = await downloader.Value.DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);

        _logger.LogDebug("Finish downloading from {url} to {htmlLocation}", url, htmlLocation);
    }
}
