namespace common.Executors;

public class LoadExecutor
{
    readonly FileWriter _fileWriter;
    readonly LeaseBroker<SeleniumDownloader> _downloaderPool;

    public LoadExecutor(FileWriter fileWriter, LeaseBroker<SeleniumDownloader> downloaderPool)
    {
        _fileWriter = fileWriter;
        _downloaderPool = downloaderPool;
    }

    public async Task LoadContentAsync(string url, string htmlLocation, CancellationToken cancellationToken)
    {
        if (File.Exists(htmlLocation))
            return;

        using var downloader = _downloaderPool.TakeLease();
        string htmlContent = await downloader.Value.DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);
    }
}
